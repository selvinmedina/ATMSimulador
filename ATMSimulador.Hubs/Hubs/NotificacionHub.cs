using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Enums;
using ATMSimulador.Domain.Security;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace ATMSimulador.Hubs.Sockets
{
    public class NotificacionHub(XmlEncryptionService xmlEncryptionService, ILogger<NotificacionHub> logger) : Hub
    {
        private static readonly ConcurrentDictionary<string, byte[]> _symmetricKeys = new ();
        private static readonly ConcurrentDictionary<int, SignalRClientDto> _clientesSignalR = new();
        private readonly XmlEncryptionService _xmlEncryptionService = xmlEncryptionService;
        private readonly RSA _rsa = RSA.Create();
        private readonly ILogger<NotificacionHub> _logger = logger;

        public override async Task OnConnectedAsync()
        {
            // Generar una clave simétrica (3DES)
            var symmetricKey = XmlEncryptionService.GenerateSymmetricKey();
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
            var encryptedSymmetricKey = XmlEncryptionService.EncryptSymmetricKeyWithRSA(symmetricKey, clientPublicKey);

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

        public static IEnumerable<SignalRClientDto>? GetClients()
        {
            return _clientesSignalR.Values.OrderByDescending(x => x.Fecha);
        }

        public static SignalRClientDto AddClient(TipoConexionCliente tipoConexionCliente, string tokenDocumentId, string connectionId)
        {
            SignalRClientDto client = new()
            {
                TipoConexionCliente = tipoConexionCliente,
                TokenDocumentId = tokenDocumentId,
                TokenConnetionId = connectionId,
                Fecha = DateTime.Now
            };

            int clientId = Guid.NewGuid().GetHashCode();
            _clientesSignalR.TryAdd(clientId, client);
            return client;
        }

        public static void RemoveClient(string tokenId)
        {
            var key = _clientesSignalR.FirstOrDefault(x => x.Value.TokenConnetionId == tokenId).Key;
            _clientesSignalR.TryRemove(key, out _);
        }

        public void ActualizarClienteConectado(int ClientTypeId, string TokenDocumentId)
        {
            try
            {
                SignalRClientDto? cliente = GetClients()?.FirstOrDefault(x => x.TokenDocumentId == TokenDocumentId);
                if (cliente != null)
                {
                    cliente.TokenConnetionId = this.Context.ConnectionId;
                    return;
                }

                AddClient((TipoConexionCliente)ClientTypeId, TokenDocumentId,
                    this.Context.ConnectionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error UpdateDataConnectedClient");
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _symmetricKeys.TryRemove(Context.ConnectionId, out _);
            RemoveClient(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        private static string ProcessXmlMessage(string xmlMessage)
        {
            //TODO: Aquí puedes agregar la lógica para procesar el mensaje XML
            return xmlMessage;
        }
    }
}
