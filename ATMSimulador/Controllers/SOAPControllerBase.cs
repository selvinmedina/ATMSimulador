using ATMSimulador.Attributes;
using ATMSimulador.SOAP;
using Microsoft.AspNetCore.Mvc;

namespace ATMSimulador.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class SOAPControllerBase : ControllerBase
    {
        public SOAPVersion SOAPVersion { get; init; }

        public SOAPControllerBase()
        {
            SOAPControllerAttribute? soapVersionAttribute = Attribute.GetCustomAttribute(GetType(), typeof(SOAPControllerAttribute)) as SOAPControllerAttribute;

            if(soapVersionAttribute is null)
                throw new Exception("A la clase derivada de SOAPControllerBase le hace falta el atributo SOAPControllerAttribute");
            else
                SOAPVersion = soapVersionAttribute.SOAPVersion;
        }

        public virtual SOAPResponseEnvelope
    }
}
