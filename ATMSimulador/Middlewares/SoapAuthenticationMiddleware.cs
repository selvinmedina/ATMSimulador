using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ATMSimulador.Middlewares
{
    public class SoapAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SoapAuthenticationMiddleware> _logger;
        private readonly IConfiguration _configuration;
        private readonly List<string> _excludedRoutes;

        public SoapAuthenticationMiddleware(RequestDelegate next, ILogger<SoapAuthenticationMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;

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

            _logger.LogInformation($"Request Path: {context.Request.Path.Value}");
            if (context.Request.Path.Value.Contains(".svc") && !IsExcludedRoute(context.Request.Path.Value))
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Unauthorized request: No token provided.");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }

                if (!ValidateToken(token, out var userId))
                {
                    _logger.LogWarning("Unauthorized request: Invalid token.");
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }

                _logger.LogInformation($"Authenticated user: {userId}");
                context.Items["userId"] = userId;
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
    }
}
