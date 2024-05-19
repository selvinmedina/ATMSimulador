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
        private readonly RSA _rsa;
        private readonly string _url;
        private byte[]? _symmetricKey;

        public SignalRClient(string url, XmlEncryptionService xmlEncryptionService)
        {
            _url = url;
            _xmlEncryptionService = xmlEncryptionService;
            _rsa = RSA.Create();
        }

        public async Task StartClient()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(_url)
                .Build();

            _connection.On<string>("ReceivePublicKey", async (serverPublicKey) =>
            {
                var clientPublicKey = Convert.ToBase64String(_rsa.ExportRSAPublicKey());
                await _connection.InvokeAsync("ReceivePublicKey", clientPublicKey);
            });

            _connection.On<string>("ReceiveSymmetricKey", (encryptedSymmetricKey) =>
            {
                _symmetricKey = _xmlEncryptionService.DecryptSymmetricKeyWithRSA(encryptedSymmetricKey, _rsa);
            });

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

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("DisposeAsync SignalRClient");

            if (_connection != null)
                await _connection.DisposeAsync();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await StartClient();
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
