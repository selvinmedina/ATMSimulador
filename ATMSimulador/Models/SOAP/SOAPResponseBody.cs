using System.Xml.Serialization;

namespace ATMSimulador.SOAP.Model
{
    [XmlType(Namespace = SOAPResponseBody.DefaultNamespace)]
    public partial class SOAPResponseBody
    {
        public const string DefultNamespacePrefix = "ser";
        public const string DefaultNamespace = "http:atm.com/service/";

        public GetLoginResponse? GetLoginResponse { get; set; }
    }
}
