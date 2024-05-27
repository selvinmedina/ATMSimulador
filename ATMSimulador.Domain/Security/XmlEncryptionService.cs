using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ATMSimulador.Domain.Security
{
    public class XmlEncryptionService
    {
        private readonly byte[] IVector = [27, 9, 45, 27, 0, 72, 171, 54];
        private readonly string _key = "ATF#.345T4TIRNGFG8FDG888434R3";

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


        public string EncryptString(string message, byte[]? key)
        {
            
        }

        public string DecryptString(string encryptedMessage, byte[]? key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var encryptedBytes = Convert.FromBase64String(encryptedMessage);

            using var des = TripleDES.Create();
            des.Key = MD5.HashData(key);
            des.IV = IVector;
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;

            using var decryptor = des.CreateDecryptor();
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public byte[] ComputeMd5Hash(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            using var des = TripleDES.Create();
            var messageBytes = Encoding.UTF8.GetBytes(input);

            des.Key = MD5.HashData(_key);
            des.IV = IVector;
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;

            using var encryptor = des.CreateEncryptor();
            var encryptedBytes = encryptor.TransformFinalBlock(messageBytes, 0, messageBytes.Length);
            return encryptedBytes;
        }
    }
}
