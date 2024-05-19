using ATMSimulador.Domain.Mensajes;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ATMSimulador.Domain.Security
{
    public class XmlEncryptionService
    {
        private readonly byte[] IVector = { 27, 9, 45, 27, 0, 72, 171, 54 };

        public string SerializeToXml<T>(T value)
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

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                xmlSerializer.Serialize(xmlWriter, value);
                return stringWriter.ToString();
            }
        }

        public T DeserializeFromXml<T>(string xml) where T : class
        {
            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentNullException(nameof(xml));

            var xmlSerializer = new XmlSerializer(typeof(T));

            using (var stringReader = new StringReader(xml))
            {
                var deserializedObject = xmlSerializer.Deserialize(stringReader);
                if (deserializedObject is T result)
                {
                    return result;
                }
                else
                {
                    throw new InvalidOperationException(XmlMessages.MS_001);
                }
            }
        }

        public string EncryptString(string message, byte[] key)
        {
            using (var des = TripleDES.Create())
            using (var md5 = MD5.Create())
            {
                var messageBytes = Encoding.UTF8.GetBytes(message);

                des.Key = md5.ComputeHash(key);
                des.IV = IVector;
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.PKCS7;

                using (var encryptor = des.CreateEncryptor())
                {
                    var encryptedBytes = encryptor.TransformFinalBlock(messageBytes, 0, messageBytes.Length);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        public string DecryptString(string encryptedMessage, byte[] key)
        {
            var encryptedBytes = Convert.FromBase64String(encryptedMessage);

            using (var des = TripleDES.Create())
            using (var md5 = MD5.Create())
            {
                des.Key = md5.ComputeHash(key);
                des.IV = IVector;
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.PKCS7;

                using (var decryptor = des.CreateDecryptor())
                {
                    var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                    return Encoding.UTF8.GetString(decryptedBytes);
                }
            }
        }

        public string ComputeMd5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
