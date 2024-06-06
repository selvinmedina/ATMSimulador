using ATMSimulador.SOAP;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace ATMSimulador.Attributes
{
    public class SOAPControllerAttribute : ProducesAttribute
    {
        public SOAPVersion SOAPVersion { get; }

        public SOAPControllerAttribute(SOAPVersion soapVersion) : base(Application.Xml)
        {
            SOAPVersion = soapVersion;
        }
    }
}
