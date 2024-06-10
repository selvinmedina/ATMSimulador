using ATMSimulador.Domain.Security;
using System.Text;
using System.Xml.Linq;

namespace ATMSimulador.Middlewares
{
    public class SoapBodyReplacementMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly EncryptionService _encryptionService;

        public SoapBodyReplacementMiddleware(RequestDelegate next, EncryptionService encryptionService)
        {
            _next = next;
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
                context.Request.EnableBuffering();
                var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                context.Request.Body.Position = 0;

                if (!string.IsNullOrEmpty(requestBody))
                {
                    var desencryptedBody = DecryptAndReplaceSoapBody(requestBody);
                    var byteArray = Encoding.UTF8.GetBytes(desencryptedBody);
                    context.Request.Body = new MemoryStream(byteArray);
                    context.Request.ContentLength = byteArray.Length;
                }
            }

            await _next(context);
        }

        private string DecryptAndReplaceSoapBody(string soapBody)
        {
            var doc = XDocument.Parse(soapBody);
            var bodyElement = doc.Descendants(XName.Get("Body", "http://schemas.xmlsoap.org/soap/envelope/")).FirstOrDefault();
            if (bodyElement != null)
            {
                foreach (var methodElement in bodyElement.Elements())
                {
                    foreach (var parameterElement in methodElement.Elements())
                    {
                        // Desencriptar elementos primitivos directamente
                        if (!parameterElement.HasElements)
                        {
                            if (!string.IsNullOrEmpty(parameterElement.Value))
                            {
                                parameterElement.Value = _encryptionService.Decrypt(parameterElement.Value);
                            }
                        }
                        else
                        {
                            DecryptXmlElement(parameterElement);
                        }
                    }
                }
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
