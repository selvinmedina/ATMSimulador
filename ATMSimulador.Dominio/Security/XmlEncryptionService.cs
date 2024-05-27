using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ATMSimulador.Dominio.Security
{
    public class XmlEncryptionService
    {
        public static string SerializeToXml<T>(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var xmlSerializer = new XmlSerializer(typeof(T));
            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false),
                Indent = true,
                OmitXmlDeclaration = true
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            xmlSerializer.Serialize(xmlWriter, value);
            return stringWriter.ToString();
        }

        public static T DeserializeFromXml<T>(string xml) where T : class
        {
            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentNullException(nameof(xml));

            var xmlSerializer = new XmlSerializer(typeof(T));

            using var stringReader = new StringReader(xml);
            var deserializedObject = xmlSerializer.Deserialize(stringReader);
            if (deserializedObject is T result)
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException("Error al deserializar el XML a la instancia del tipo esperado.");
            }
        }
    }
}
