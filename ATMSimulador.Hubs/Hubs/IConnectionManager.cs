using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Enums;

namespace ATMSimulador.Hubs.Hubs
{
    public interface IConnectionManager
    {
        byte[] GenerateAndStoreSymmetricKey(string connectionId);
        byte[] GetSymmetricKey(string connectionId);
        void RemoveSymmetricKey(string connectionId);
        IEnumerable<SignalRClientDto> GetClients();
        SignalRClientDto AddClient(TipoConexionCliente tipoConexionCliente, string tokenDocumentId, string connectionId);
        void RemoveClient(string tokenId);
        byte[] UpdateClientConnection(int clientTypeId, string tokenDocumentId, string connectionId);
    }
}
