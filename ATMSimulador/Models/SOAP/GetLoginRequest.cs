using System.Xml.Serialization;

namespace ATMSimulador.SOAP.Model
{
    public class GetLoginRequest
    {
        [XmlElement("NombreUsuario")]
        public string? NombreUsuario { get; set; }

        [XmlElement("Pin")]
        public string? Pin { get; set; }
    }
}
