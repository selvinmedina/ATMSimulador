using ATMSimulador.Domain.Security;
using Newtonsoft.Json;
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
                // Desencriptar el cuerpo del mensaje SOAP
                context.Request.EnableBuffering();
                var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!string.IsNullOrEmpty(requestBody))
                {
                    var desencryptedObjects = DecryptSoapBodyToObjects(requestBody);
                    foreach (var keyValue in desencryptedObjects)
                    {
                        // Convertir el valor a JSON si es un objeto DTO
                        if (keyValue.Key.EndsWith("Dto", StringComparison.OrdinalIgnoreCase) && keyValue.Value is Dictionary<string, object> dtoValue)
                        {
                            context.Items[keyValue.Key] = JsonConvert.SerializeObject(ToCamelCaseDictionary(dtoValue));
                        }
                        else
                        {
                            context.Items[keyValue.Key] = keyValue.Value;
                        }
                    }
                }
            }

            await _next(context);
        }

        private Dictionary<string, object> DecryptSoapBodyToObjects(string soapBody)
        {
            var desencryptedValues = new Dictionary<string, object>();
            var doc = XDocument.Parse(soapBody);
            var bodyElement = doc.Descendants(XName.Get("Body", "http://schemas.xmlsoap.org/soap/envelope/")).FirstOrDefault();
            if (bodyElement != null)
            {
                foreach (var methodElement in bodyElement.Elements())
                {
                    foreach (var parameterElement in methodElement.Elements())
                    {
                        var key = parameterElement.Name.LocalName;
                        var value = DecryptXmlElementToObject(parameterElement);
                        desencryptedValues[key] = value;
                    }
                }
            }

            return desencryptedValues;
        }

        private object DecryptXmlElementToObject(XElement element)
        {
            if (element.HasElements)
            {
                var dict = new Dictionary<string, object>();
                foreach (var subElement in element.Elements())
                {
                    dict[subElement.Name.LocalName] = DecryptXmlElementToObject(subElement);
                }
                return dict;
            }
            else
            {
                return _encryptionService.Decrypt(element.Value);
            }
        }

        private Dictionary<string, object> ToCamelCaseDictionary(Dictionary<string, object> dict)
        {
            return dict.ToDictionary(kv => Char.ToLowerInvariant(kv.Key[0]) + kv.Key.Substring(1), kv => kv.Value);
        }
    }
}
