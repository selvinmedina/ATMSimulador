using ATMSimulador.Attributes;
using ATMSimulador.SOAP;
using ATMSimulador.SOAP.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace ATMSimulador.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class SOAPControllerBase : ControllerBase
    {
        public SOAPVersion SOAPVersion { get; init; }
        private readonly IWebHostEnvironment _env;

        public SOAPControllerBase(IWebHostEnvironment env)
        {
            _env = env;

            SOAPControllerAttribute? soapVersionAttribute = Attribute.GetCustomAttribute(GetType(), typeof(SOAPControllerAttribute)) as SOAPControllerAttribute;

            if (soapVersionAttribute is null)
                throw new Exception("A la clase derivada de SOAPControllerBase le hace falta el atributo SOAPControllerAttribute");
            else
                SOAPVersion = soapVersionAttribute.SOAPVersion;
        }

        public virtual SOAPResponseEnvelope CreateSOAPResponseEnvelope() => SOAPVersion == SOAPVersion.v1_1 ? new SoapResponseEnvelope1_1() : new SoapResponseEnvelope1_2();

        protected ActionResult ProcessWsdlFile(string path)
        {
            var _baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            // convertir el path virtual a fisico

            if (path.StartsWith("~"))
                path = path.Replace("~", _env.ContentRootPath);

            String content;

            try
            {
                content = System.IO.File.ReadAllText(path, Encoding.UTF8);
            }
            catch (DirectoryNotFoundException)
            {
                // TODO: Deberia ser un SOAPFault
                return new ObjectResult("directorio wsdl no encontrado") { StatusCode = StatusCodes.Status500InternalServerError };
            }
            catch (FileNotFoundException)
            {
                // TODO: Should be a SOAPFault
                return new ObjectResult("El archivo wsdl no fue encontrado.") { StatusCode = StatusCodes.Status500InternalServerError };
            }

            // Reemplazar placeholder con los valores actuales
            content = content.Replace("{SERVICE_URL}", _baseUrl);

            return File(Encoding.UTF8.GetBytes(content), "text/xml;charset=UTF-8");
        }

        #region WSDL Handling


        [HttpGet]
        public IActionResult Get(string? wsdl, string? xsd)
        {
            var controllerName = ControllerContext.RouteData.Values["controller"]?.ToString();

            if (wsdl is not null)
                return ProcessWsdlFile($"~/wsdl/${(controllerName is null ? "" : controllerName)}/{wsdl == string.Empty}.xml");

            if (xsd is not null)
            {
                if (xsd == string.Empty)
                    // TODO: Deberia ser
                    return BadRequest("El parametro xsd no puede estar vacio");
                else
                    return ProcessWsdlFile($"~/wsdl/${(controllerName is null ? "" : controllerName)}/{xsd}.xml");
            }

            // TODO: Seberia ser SOAPFault

            return BadRequest("Request invalido");
        }
        #endregion
    }
}
