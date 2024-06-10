using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Xml.Linq;

namespace ATMSimulador.Middlewares
{
    public class TokenExtractionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TokenExtractionMiddleware> _logger;
        private readonly IConfiguration _configuration;
        private readonly List<string> _excludedRoutes;

        public TokenExtractionMiddleware(RequestDelegate next, ILogger<TokenExtractionMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;

            _excludedRoutes = new List<string>
            {
                "Registro",
                "Login"
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Request == null)
            {
                throw new ArgumentNullException(nameof(context.Request));
            }

            if (context.Request.Path == null)
            {
                throw new ArgumentNullException(nameof(context.Request.Path));
            }

            if (context.Request.Path.Value == null)
            {
                throw new ArgumentNullException(nameof(context.Request.Path.Value));
            }

            if (context.Request.Path.Value.Contains(".svc") && !await IsExcludedRouteAsync(context))
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
            }

            await _next(context);
        }

        private async Task<bool> IsExcludedRouteAsync(HttpContext context)
        {
            // Check the method name in the SOAP body
            context.Request.EnableBuffering();
            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrEmpty(requestBody))
            {
                var doc = XDocument.Parse(requestBody);
                var bodyElement = doc.Descendants(XName.Get("Body", "http://schemas.xmlsoap.org/soap/envelope/")).FirstOrDefault();
                if (bodyElement != null)
                {
                    foreach (var methodElement in bodyElement.Elements())
                    {
                        var methodName = methodElement.Name.LocalName;
                        if (_excludedRoutes.Any(route => route.Equals(methodName, StringComparison.OrdinalIgnoreCase)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
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
    }
}
