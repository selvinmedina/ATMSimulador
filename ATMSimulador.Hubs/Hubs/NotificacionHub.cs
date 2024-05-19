using ATMSimulador.Domain.Security;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace ATMSimulador.Hubs.Sockets
{
    public class NotificacionHub : Hub
    {
        private static readonly ConcurrentDictionary<string, byte[]> _symmetricKeys = new ConcurrentDictionary<string, byte[]>();

        private readonly XmlEncryptionService _xmlEncryptionService;
        private readonly RSA _rsa;

        public NotificacionHub(XmlEncryptionService xmlEncryptionService)
        {
            _xmlEncryptionService = xmlEncryptionService;
            _rsa = RSA.Create();
        }

        public override async Task OnConnectedAsync()
        {
            // Generar una clave simétrica (3DES)
            var symmetricKey = _xmlEncryptionService.GenerateSymmetricKey();
            _symmetricKeys[Context.ConnectionId] = symmetricKey;

            // Obtener la clave pública del cliente y encriptar la clave simétrica con ella
            var publicKey = Convert.ToBase64String(_rsa.ExportRSAPublicKey());
            await Clients.Caller.SendAsync("ReceivePublicKey", publicKey);

            await base.OnConnectedAsync();
        }

        public async Task ReceivePublicKey(string clientPublicKey)
        {
            if (!_symmetricKeys.TryGetValue(Context.ConnectionId, out var symmetricKey))
            {
                throw new InvalidOperationException("Symmetric key not found for connection.");
            }

            // Encriptar la clave simétrica usando la clave pública RSA del cliente
            var encryptedSymmetricKey = _xmlEncryptionService.EncryptSymmetricKeyWithRSA(symmetricKey, clientPublicKey);

            // Enviar la clave simétrica encriptada al cliente
            await Clients.Caller.SendAsync("ReceiveSymmetricKey", encryptedSymmetricKey);
        }

        public async Task SendMessage(string encryptedMessage)
        {
            if (!_symmetricKeys.TryGetValue(Context.ConnectionId, out var symmetricKey))
            {
                throw new InvalidOperationException("Symmetric key has not been received.");
            }

            var decryptedMessage = _xmlEncryptionService.DecryptString(encryptedMessage, symmetricKey);
            var xmlMessage = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(decryptedMessage));

            // Procesar el mensaje XML
            var responseMessage = ProcessXmlMessage(xmlMessage);

            var encryptedResponse = _xmlEncryptionService.EncryptString(responseMessage, symmetricKey);
            await Clients.Caller.SendAsync("ReceiveMessage", encryptedResponse);
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _symmetricKeys.TryRemove(Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }

        private string ProcessXmlMessage(string xmlMessage)
        {
            // Aquí puedes agregar la lógica para procesar el mensaje XML
            return "<Response>...</Response>";
        }
    }
}
