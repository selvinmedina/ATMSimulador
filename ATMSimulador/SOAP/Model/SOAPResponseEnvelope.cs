using System.Xml.Serialization;

namespace ATMSimulador.SOAP.Model
{
    [XmlRoot("Envelope", Namespace = SOAPConstants.SOAP_1_Namespace)]
    public partial class SoapResponseEnvelope1_1: SOAPResponseEnvelope { }

    [XmlRoot("Envelope", Namespace = SOAPConstants.SOAP_2_Namespace)]
    public partial class SoapResponseEnvelope1_2 : SOAPResponseEnvelope { }

    public partial class SOAPResponseEnvelope
    {
        protected SOAPResponseBody? _body;

        public SOAPResponseBody Body
        {
            get
            {
                if (_body is null)
                    _body = new SOAPResponseBody();

                return _body;
            }
            set
            {
                _body = value;
            }
        }
    }
}
