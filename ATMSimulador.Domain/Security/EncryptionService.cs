using System;
using System.Security.Cryptography;
using System.Text;

namespace ATMSimulador.Domain.Security
{
    public class EncryptionService
    {
        private readonly byte[] IVector = { 27, 9, 45, 27, 0, 72, 171, 54 };

        public string EncryptString(string str, string key)
        {
            using (var tDes = TripleDES.Create())
            using (var md5 = MD5.Create())
            {
                var enc = new ASCIIEncoding();
                var byteData = enc.GetBytes(str);

                tDes.Key = md5.ComputeHash(enc.GetBytes(key));
                tDes.IV = IVector;

                using (var transform = tDes.CreateEncryptor())
                {
                    var result = transform.TransformFinalBlock(byteData, 0, byteData.Length);
                    return Convert.ToBase64String(result);
                }
            }
        }

        public string DecryptString(string base64Str, string key)
        {
            using (var tDes = TripleDES.Create())
            using (var md5 = MD5.Create())
            {
                var enc = new ASCIIEncoding();
                var encData = Convert.FromBase64String(base64Str);

                tDes.Key = md5.ComputeHash(enc.GetBytes(key));
                tDes.IV = IVector;

                using (var transform = tDes.CreateDecryptor())
                {
                    var result = transform.TransformFinalBlock(encData, 0, encData.Length);
                    return enc.GetString(result);
                }
            }
        }

        public string ComputeMd5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var enc = new ASCIIEncoding();
                var inputBytes = enc.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}
