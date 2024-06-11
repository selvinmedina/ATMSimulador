using ATMSimulador.Domain.Security;

namespace ATMSimulador.Middlewares
{
    public class HmacValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HmacValidationMiddleware> _logger;
        private readonly EncryptionService _encryptionService;
        private readonly List<string> _excludedRoutes;

        public HmacValidationMiddleware(RequestDelegate next, ILogger<HmacValidationMiddleware> logger, EncryptionService encryptionService)
        {
            _next = next;
            _logger = logger;
            _encryptionService = encryptionService;

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

            if (context.Request.Path.Value.Contains(".svc"))
            {
                // Exclude specified routes from HMAC validation
                context.Request.EnableBuffering();
                var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;

                var soapAction = context.Request.Headers["SOAPAction"].FirstOrDefault();
                if (!string.IsNullOrEmpty(soapAction))
                {
                    var methodName = soapAction.Split('/').Last();
                    if (_excludedRoutes.Contains(methodName))
                    {
                        await _next(context);
                        return;
                    }
                }

                var receivedHmac = context.Request.Headers["X-HMAC-Signature"].FirstOrDefault();
                if (string.IsNullOrEmpty(receivedHmac))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized - Missing HMAC signature");
                    return;
                }

                if (!_encryptionService.VerifyHmacMd5(requestBody, receivedHmac))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized - Invalid HMAC signature");
                    return;
                }
            }

            await _next(context);
        }
    }
}
