using System.Xml.Serialization;

namespace ATMSimulador.SOAP.Model
{
    public partial class SOAPRequestBody
    {
        [XmlElement("GetLoginRequest", Namespace = "http:atm.com/service/")]
        public GetLoginRequest? LoginRequest { get; set; }
    }
}
