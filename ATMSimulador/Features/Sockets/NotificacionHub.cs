using ATMSimulador.Domain.Security;
using Microsoft.AspNetCore.SignalR;
using System.Text;

namespace ATMSimulador.Features.Sockets
{
    public class NotificacionHub : Hub
    {
        private readonly KeyService _keyService;
        private readonly XmlEncryptionService _xmlEncryptionService;
        private byte[]? _symmetricKey;

        public NotificacionHub(XmlEncryptionService xmlEncryptionService, KeyService keyService)
        {
            _xmlEncryptionService = xmlEncryptionService;
            _keyService = keyService;
        }

        public override async Task OnConnectedAsync()
        {
            // Enviar la clave pública al cliente cuando se conecta
            var publicKey = _keyService.GetPublicKey();
            await Clients.Caller.SendAsync("ReceivePublicKey", publicKey);
            await base.OnConnectedAsync();
        }

        public async Task SendSymmetricKey(string encryptedSymmetricKey)
        {
            // Desencriptar la clave simétrica usando la clave privada RSA
            var encryptedKeyBytes = Convert.FromBase64String(encryptedSymmetricKey);
            _symmetricKey = _keyService.DecryptData(encryptedKeyBytes);
        }

        public async Task SendMessage(string encryptedMessage)
        {
            var decryptedMessage = _xmlEncryptionService.DecryptString(encryptedMessage, _symmetricKey);
            var xmlMessage = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(decryptedMessage));

            // Procesar el mensaje XML
            var responseMessage = ProcessXmlMessage(xmlMessage);

            var encryptedResponse = _xmlEncryptionService.EncryptString(responseMessage, _symmetricKey);
            await Clients.Caller.SendAsync("ReceiveMessage", encryptedResponse);
        }

        private string ProcessXmlMessage(string xmlMessage)
        {
            // Aquí puedes agregar la lógica para procesar el mensaje XML
            return "<Response>...</Response>";
        }
    }
}
