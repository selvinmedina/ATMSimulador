using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Cryptography;
using System.Text;

namespace ATMSimulador.Features.Sockets
{
    public class SignalRClient : IAsyncDisposable
    {
        private HubConnection? _connection;

        public async Task StartClient(string chatHubUrl)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(chatHubUrl)
                .Build();

            _connection.On<string>("ReceiveMessage", (encryptedMessage) =>
            {
                var decryptedMessage = Decrypt(encryptedMessage);
                var responseMessage = Encoding.UTF8.GetString(decryptedMessage);

                // Procesar el mensaje de respuesta
                Console.WriteLine("Mensaje recibido: " + responseMessage);
            });

            await _connection.StartAsync();

            var message = "<Request>...</Request>";
            var encryptedMessage = Encrypt(message);
            await _connection.InvokeAsync("SendMessage", encryptedMessage);
        }

        private string Encrypt(string message)
        {
            using (var des = TripleDES.Create())
            {
                des.Key = GetKey();
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.PKCS7;

                var messageBytes = Encoding.UTF8.GetBytes(message);
                using (var encryptor = des.CreateEncryptor())
                {
                    var encryptedBytes = encryptor.TransformFinalBlock(messageBytes, 0, messageBytes.Length);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        private byte[] Decrypt(string encryptedMessage)
        {
            var messageBytes = Convert.FromBase64String(encryptedMessage);

            using (var des = TripleDES.Create())
            {
                des.Key = GetKey();
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.PKCS7;

                using (var decryptor = des.CreateDecryptor())
                {
                    return decryptor.TransformFinalBlock(messageBytes, 0, messageBytes.Length);
                }
            }
        }

        private byte[] GetKey()
        {
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(Encoding.UTF8.GetBytes("YourSecretKey"));
            }
        }

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("DisposeAsync SignalRClient");

            if (_connection != null)
                await _connection.DisposeAsync();
        }
    }
}
