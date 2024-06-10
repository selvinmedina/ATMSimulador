using ATMSimulador.Domain.Security;
using System.Xml.Linq;

namespace ATMSimulador.Middlewares
{
    public class SoapDecryptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SoapDecryptionMiddleware> _logger;
        private readonly EncryptionService _encryptionService;

        public SoapDecryptionMiddleware(RequestDelegate next, ILogger<SoapDecryptionMiddleware> logger, EncryptionService encryptionService)
        {
            _next = next;
            _logger = logger;
            _encryptionService = encryptionService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value.Contains(".svc"))
            {
                // Desencriptar el cuerpo del mensaje SOAP
                context.Request.EnableBuffering();
                var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!string.IsNullOrEmpty(requestBody))
                {
                    var desencryptedValues = DecryptSoapBody(requestBody);
                    foreach (var keyValue in desencryptedValues)
                    {
                        context.Items[keyValue.Key] = keyValue.Value;
                    }
                }
            }

            await _next(context);
        }

        private Dictionary<string, string> DecryptSoapBody(string soapBody)
        {
            var desencryptedValues = new Dictionary<string, string>();
            var doc = XDocument.Parse(soapBody);
            var bodyElement = doc.Descendants(XName.Get("Body", "http://schemas.xmlsoap.org/soap/envelope/")).FirstOrDefault();
            if (bodyElement != null)
            {
                DecryptXmlElement(bodyElement, desencryptedValues);
            }

            return desencryptedValues;
        }

        private void DecryptXmlElement(XElement element, Dictionary<string, string> desencryptedValues)
        {
            foreach (var subElement in element.Elements())
            {
                if (!string.IsNullOrEmpty(subElement.Value))
                {
                    var desencryptedValue = _encryptionService.Decrypt(subElement.Value);
                    desencryptedValues[subElement.Name.LocalName] = desencryptedValue;
                }

                if (subElement.HasElements)
                {
                    DecryptXmlElement(subElement, desencryptedValues);
                }
            }
        }
    }
}
