using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain;
using ATMSimulador.Domain.Security;
using ATMSimulador.Hubs.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using ATMSimulador.Domain.Enums;

namespace ATMSimulador.Hubs.Sockets
{
    public class NotificacionHub(IConnectionManager connectionManager, XmlEncryptionService xmlEncryptionService, ILogger<NotificacionHub> logger) : Hub
    {
        private readonly IConnectionManager _connectionManager= connectionManager;
        private readonly XmlEncryptionService _xmlEncryptionService = xmlEncryptionService;
        private readonly ILogger<NotificacionHub> _logger = logger;

        public override async Task OnConnectedAsync()
        {
            // Generar una clave simétrica (3DES)
            _connectionManager.GenerateAndStoreSymmetricKey(Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _connectionManager.RemoveSymmetricKey(Context.ConnectionId);
            _connectionManager.RemoveClient(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string encryptedMessage)
        {
            var symmetricKey = _connectionManager.GetSymmetricKey(Context.ConnectionId);

            var decryptedMessage = _xmlEncryptionService.DecryptString(encryptedMessage, symmetricKey);
            var xmlMessage = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(decryptedMessage));

            // Procesar el mensaje XML
            var responseMessage = ProcessXmlMessage(xmlMessage);

            var encryptedResponse = _xmlEncryptionService.EncryptString(responseMessage, symmetricKey);
            await Clients.Caller.SendAsync("ReceiveMessage", encryptedResponse);
        }

        public void ActualizarClienteConectado(int clientTypeId, string tokenDocumentId)
        {
            try
            {
                var symmetricKey = _connectionManager.UpdateClientConnection(clientTypeId, tokenDocumentId, Context.ConnectionId);

                var symmetricKeyBase64 = Convert.ToBase64String(symmetricKey);

                Clients.Client(Context.ConnectionId).SendAsync("ReceiveSymmetricKey", symmetricKeyBase64);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error UpdateDataConnectedClient");
            }
        }

        public async Task RealizarOperacion(string operacion, string tokenDocumentoId, string dataEncriptada)
        {
            try
            {
                //var dataDesencriptada = _xmlEncryptionService.DecryptString(dataEncriptada, _connectionManager.GetSymmetricKey(Context.ConnectionId));
                //var dtoTransfer = XmlEncryptionService.DeserializeFromXml<SignalRTransferDto>(dataDesencriptada);

                var clienteOrigen = _connectionManager.GetClients().FirstOrDefault(x => x.TokenDocumentId == tokenDocumentoId);
                var clienteDestino = _connectionManager.GetClients().First(x => x.TipoConexionCliente == TipoConexionCliente.ApiATM);

                if (clienteOrigen == null)
                {
                    clienteOrigen = _connectionManager.AddClient(TipoConexionCliente.FrontendATM, tokenDocumentoId, Context.ConnectionId);
                    if (clienteOrigen == null)
                    {
                        _logger.LogError("Error al registrar operación, cliente origen no encontrado.");
                        await Clients.Client(Context.ConnectionId).SendAsync(operacion, dataEncriptada);
                        return;
                    }
                }

                await Clients.Client(clienteDestino.TokenConnetionId).SendAsync(operacion, dataEncriptada);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error RealizarOperacion");
                throw;
            }
        }
        private static string ProcessXmlMessage(string xmlMessage)
        {
            //TODO: Aquí puedes agregar la lógica para procesar el mensaje XML
            return xmlMessage;
        }
    }
}
