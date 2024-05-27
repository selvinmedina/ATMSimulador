using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Enums;
using System.Collections.Concurrent;

namespace ATMSimulador.Hubs.Hubs
{
    public class ConnectionManager : IConnectionManager
    {
        private static readonly ConcurrentDictionary<int, SignalRClientDto> _clientesSignalR = new();

        public IEnumerable<SignalRClientDto> GetClients()
        {
            return _clientesSignalR.Values.OrderByDescending(x => x.Fecha);
        }

        public SignalRClientDto AddClient(TipoConexionCliente tipoConexionCliente, string tokenDocumentId, string connectionId)
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

        public void RemoveClient(string tokenId)
        {
            var key = _clientesSignalR.FirstOrDefault(x => x.Value.TokenConnetionId == tokenId).Key;
            _clientesSignalR.TryRemove(key, out _);
        }

        public void UpdateClientConnection(int clientTypeId, string tokenDocumentId, string connectionId)
        {
            var client = _clientesSignalR.Values.FirstOrDefault(x => x.TokenDocumentId == tokenDocumentId);
            if (client != null)
            {
                client.TokenConnetionId = connectionId;
            }
            else
            {
                AddClient((TipoConexionCliente)clientTypeId, tokenDocumentId, connectionId);
            }
        }
    }
}
