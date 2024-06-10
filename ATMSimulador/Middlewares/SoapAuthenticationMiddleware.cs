using ATMSimulador.Domain.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Xml.Linq;

namespace ATMSimulador.Middlewares
{
    public class SoapAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SoapAuthenticationMiddleware> _logger;
        private readonly IConfiguration _configuration;
        private readonly EncryptionService _encryptionService;
        private readonly List<string> _excludedRoutes;

        public SoapAuthenticationMiddleware(RequestDelegate next, ILogger<SoapAuthenticationMiddleware> logger, IConfiguration configuration, EncryptionService encryptionService)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
            _encryptionService = encryptionService;

            _excludedRoutes = new List<string>
            {
                "/UsuariosService.svc/Registro",
                "/UsuariosService.svc/Login"
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Request.Path == null)
            {
                throw new ArgumentNullException(nameof(context.Request.Path));
            }

            if (context.Request.Path.Value == null)
            {
                throw new ArgumentNullException(nameof(context.Request.Path.Value));
            }

            if (context.Request.Path.Value.Contains(".svc") && !IsExcludedRoute(context.Request.Path.Value))
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (string.IsNullOrEmpty(token))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }

                if (!ValidateToken(token, out var userId))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }

                context.Items["userId"] = userId;

                // Desencriptar el cuerpo del mensaje SOAP
                context.Request.EnableBuffering();
                var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!string.IsNullOrEmpty(requestBody))
                {
                    var desencryptedBody = DecryptSoapBody(requestBody);
                    var byteArray = Encoding.UTF8.GetBytes(desencryptedBody);
                    context.Request.Body = new MemoryStream(byteArray);
                    context.Request.ContentLength = byteArray.Length;
                }
            }

            await _next(context);
        }

        private bool IsExcludedRoute(string path)
        {
            return _excludedRoutes.Any(route => path.Equals(route, StringComparison.OrdinalIgnoreCase));
        }

        private bool ValidateToken(string token, out string? userId)
        {
            userId = null;
            var secretKey = _configuration["Security:Jwt:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentNullException(nameof(secretKey), "The secret key cannot be null or empty.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secretKey);

            try
            {
                var claims = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Security:Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Security:Jwt:Audience"],
                    ValidateLifetime = true
                }, out var validatedToken);

                userId = claims.FindFirst("userId")?.Value;

                return userId is { };
            }
            catch
            {
                _logger.LogError("Token validation failed.");
                return false;
            }
        }
        private string DecryptSoapBody(string soapBody)
        {
            var doc = XDocument.Parse(soapBody);
            var bodyElement = doc.Descendants(XName.Get("Body", "http://schemas.xmlsoap.org/soap/envelope/")).FirstOrDefault();
            if (bodyElement != null)
            {
                DecryptXmlElement(bodyElement);
            }

            return doc.ToString();
        }

        private void DecryptXmlElement(XElement element)
        {
            foreach (var subElement in element.Elements())
            {
                if (!string.IsNullOrEmpty(subElement.Value))
                {
                    subElement.Value = _encryptionService.Decrypt(subElement.Value);
                }

                if (subElement.HasElements)
                {
                    DecryptXmlElement(subElement);
                }
            }
        }
    }
}
