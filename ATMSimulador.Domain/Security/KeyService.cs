using System.Security.Cryptography;

namespace ATMSimulador.Domain.Security
{
    public class KeyService
    {
        private readonly RSA _rsa;

        public KeyService()
        {
            _rsa = RSA.Create();
        }

        public string GetPublicKey()
        {
            return Convert.ToBase64String(_rsa.ExportRSAPublicKey());
        }

        public byte[] DecryptData(byte[] data)
        {
            return _rsa.Decrypt(data, RSAEncryptionPadding.OaepSHA256);
        }
    }
}
