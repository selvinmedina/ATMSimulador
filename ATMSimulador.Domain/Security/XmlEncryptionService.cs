using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ATMSimulador.Domain.Security
{
    public class XmlEncryptionService
    {
        private readonly byte[] IVector = [27, 9, 45, 27, 0, 72, 171, 54];

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

        public static byte[] GenerateSymmetricKey()
        {
            using var des = TripleDES.Create();
            des.GenerateKey();
            return des.Key;
        }

        public string EncryptString(string message, byte[]? key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            using var des = TripleDES.Create();
            var messageBytes = Encoding.UTF8.GetBytes(message);

            des.Key = MD5.HashData(key);
            des.IV = IVector;
            des.Mode = CipherMode.ECB;
            des.Padding = PaddingMode.PKCS7;

            using var encryptor = des.CreateEncryptor();
            var encryptedBytes = encryptor.TransformFinalBlock(messageBytes, 0, messageBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
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

        public static byte[] ComputeMd5Hash(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = MD5.HashData(inputBytes);
            return hashBytes;
        }
    }
}
