﻿using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Enums;
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Security;
using System.Collections.Concurrent;

namespace ATMSimulador.Hubs.Hubs
{
    public class ConnectionManager : IConnectionManager
    {
        private static readonly ConcurrentDictionary<string, byte[]> _symmetricKeys = new();
        private static readonly ConcurrentDictionary<int, SignalRClientDto> _clientesSignalR = new();

        public void GenerateAndStoreSymmetricKey(string connectionId)
        {
            var symmetricKey = XmlEncryptionService.GenerateSymmetricKey();
            _symmetricKeys[connectionId] = symmetricKey;
        }

        public byte[] GetSymmetricKey(string connectionId)
        {
            return _symmetricKeys.TryGetValue(connectionId, out var key) ? key : throw new InvalidOperationException(SecurityMensajes.MSEC_001);
        }

        public void RemoveSymmetricKey(string connectionId)
        {
            _symmetricKeys.TryRemove(connectionId, out _);
        }

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
