using System.Security.Cryptography;
using System.Text;

namespace ATMSimulador.Domain.Security
{
    public class EncryptionService
    {
        private readonly string _key;

        public EncryptionService(string key)
        {
            if (key.Length != 24)
                throw new ArgumentException("Key length must be 24 characters.");

            _key = key;
        }

        public string Encrypt(string plainText)
        {
            using var tripleDes = TripleDES.Create();
            tripleDes.Key = Encoding.UTF8.GetBytes(_key);
            tripleDes.Mode = CipherMode.ECB;
            tripleDes.Padding = PaddingMode.PKCS7;

            var data = Encoding.UTF8.GetBytes(plainText);

            using var encryptor = tripleDes.CreateEncryptor();
            var results = encryptor.TransformFinalBlock(data, 0, data.Length);
            return Convert.ToBase64String(results);
        }

        public byte[] EncryptBytes(string plainText)
        {
            using var tripleDes = TripleDES.Create();
            tripleDes.Key = Encoding.UTF8.GetBytes(_key);
            tripleDes.Mode = CipherMode.ECB;
            tripleDes.Padding = PaddingMode.PKCS7;

            var data = Encoding.UTF8.GetBytes(plainText);

            using var encryptor = tripleDes.CreateEncryptor();
            var results = encryptor.TransformFinalBlock(data, 0, data.Length);
            return results;
        }

        public byte[] Encrypt(byte[] plainBytes)
        {
            using var tripleDes = TripleDES.Create();
            tripleDes.Key = Encoding.UTF8.GetBytes(_key);
            tripleDes.Mode = CipherMode.ECB;
            tripleDes.Padding = PaddingMode.PKCS7;

            using var encryptor = tripleDes.CreateEncryptor();
            var results = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return results;
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                using var tripleDes = TripleDES.Create();
                tripleDes.Key = Encoding.UTF8.GetBytes(_key);
                tripleDes.Mode = CipherMode.ECB;
                tripleDes.Padding = PaddingMode.PKCS7;

                var data = Convert.FromBase64String(cipherText);

                using var decryptor = tripleDes.CreateDecryptor();
                var results = decryptor.TransformFinalBlock(data, 0, data.Length);
                return Encoding.UTF8.GetString(results);
            }
            catch (Exception)
            {
                throw new ArgumentException("El texto cifrado proporcionado no está encriptado correctamente o la clave de encriptación es incorrecta.");
            }
        }

        public byte[] DecryptBytes(string cipherText)
        {
            using var tripleDes = TripleDES.Create();
            tripleDes.Key = Encoding.UTF8.GetBytes(_key);
            tripleDes.Mode = CipherMode.ECB;
            tripleDes.Padding = PaddingMode.PKCS7;

            var data = Convert.FromBase64String(cipherText);

            using var decryptor = tripleDes.CreateDecryptor();
            var results = decryptor.TransformFinalBlock(data, 0, data.Length);
            return results;
        }

        public string Decrypt(byte[] cipherBytes)
        {
            using var tripleDes = TripleDES.Create();
            tripleDes.Key = Encoding.UTF8.GetBytes(_key);
            tripleDes.Mode = CipherMode.ECB;
            tripleDes.Padding = PaddingMode.PKCS7;

            using var decryptor = tripleDes.CreateDecryptor();
            var results = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(results);
        }

        public bool VerifyHmacMd5(string key, string message, string receivedHmac)
        {
            using HMACMD5 hmac = new(Encoding.UTF8.GetBytes(key));
            byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            string computedHmac = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return computedHmac == receivedHmac;
        }
    }
}
