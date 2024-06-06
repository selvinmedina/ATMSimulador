using System.Xml.Serialization;

namespace ATMSimulador.SOAP.Model
{
    [XmlRoot("Envelope", Namespace = SOAPConstants.SOAP_1_Namespace)]
    public partial class SOAP1_1RequestEnvelope: SOAPRequestEnvelope { }

    
    [XmlRoot("Envelope", Namespace = SOAPConstants.SOAP_2_Namespace)]
    public partial class SOAP1_2RequestEnvelope: SOAPRequestEnvelope { }

    public partial class SOAPRequestEnvelope
    {
        public SOAPHeader? Header { get; set; }
        public SOAPRequestBody? Body { get; set; }
    }
}
