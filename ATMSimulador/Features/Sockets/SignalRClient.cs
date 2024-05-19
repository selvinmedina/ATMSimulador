using ATMSimulador.Domain.Security;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Cryptography;
using System.Text;

namespace ATMSimulador.Features.Sockets
{
    public class SignalRClient : IAsyncDisposable, IHostedService
    {
        private HubConnection? _connection;
        private readonly XmlEncryptionService _xmlEncryptionService;
        private string _url;
        private byte[]? _symmetricKey;

        public SignalRClient(string url, XmlEncryptionService xmlEncryptionService)
        {
            _url = url;
            _xmlEncryptionService = xmlEncryptionService;
        }

        public async Task StartClient(string chatHubUrl)
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(chatHubUrl)
                .Build();

            _connection.On<string>("ReceiveMessage", (encryptedMessage) =>
            {
                if (_symmetricKey == null)
                {
                    Console.WriteLine("Clave simétrica no recibida.");
                    return;
                }

                var decryptedMessage = _xmlEncryptionService.DecryptString(encryptedMessage, _symmetricKey);
                var responseMessage = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(decryptedMessage));

                // Procesar el mensaje de respuesta
                Console.WriteLine("Mensaje recibido: " + responseMessage);
            });

            _connection.On<string>("ReceivePublicKey", async (publicKey) =>
            {
                // Generar una clave simétrica (3DES)
                _symmetricKey = GenerateSymmetricKey();

                // Encriptar la clave simétrica usando la clave pública RSA
                var encryptedSymmetricKey = EncryptSymmetricKeyWithRSA(_symmetricKey, publicKey);

                // Enviar la clave simétrica encriptada al servidor
                await _connection.InvokeAsync("SendSymmetricKey", encryptedSymmetricKey);
            });
            await Task.Delay(TimeSpan.FromSeconds(15));
            await _connection.StartAsync();

            // Esperar hasta que la clave simétrica haya sido establecida antes de enviar mensajes
            while (_symmetricKey == null)
            {
                await Task.Delay(100);
            }

            var message = "<Request>...</Request>";
            var encryptedMessage = _xmlEncryptionService.EncryptString(message, _symmetricKey);
            await _connection.InvokeAsync("SendMessage", encryptedMessage);
        }

        private byte[] GenerateSymmetricKey()
        {
            using (var des = TripleDES.Create())
            {
                des.GenerateKey();
                return des.Key;
            }
        }

        private string EncryptSymmetricKeyWithRSA(byte[] symmetricKey, string publicKeyBase64)
        {
            var publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(publicKeyBytes, out _);
                var encryptedKey = rsa.Encrypt(symmetricKey, RSAEncryptionPadding.OaepSHA256);
                return Convert.ToBase64String(encryptedKey);
            }
        }

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("DisposeAsync SignalRClient");

            if (_connection != null)
                await _connection.DisposeAsync();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await StartClient(_url);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("StopAsync SignalRClient");

            if (_connection != null)
            {
                return _connection.StopAsync(cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}
