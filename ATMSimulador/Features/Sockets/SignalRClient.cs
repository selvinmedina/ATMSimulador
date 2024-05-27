using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Enums;
using ATMSimulador.Domain.Security;
using ATMSimulador.Features.Usuarios.Mediator.Registro;
using MediatR;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;

namespace ATMSimulador.Features.Sockets
{
    public class SignalRClient(string url, XmlEncryptionService xmlEncryptionService, IMediator mediator) : IAsyncDisposable, IHostedService
    {
        private HubConnection? _connection;
        private readonly XmlEncryptionService _xmlEncryptionService = xmlEncryptionService;
        private readonly string _url = url;
        private readonly string _tokenDocumentId = Guid.NewGuid().ToString();
        private readonly IMediator _mediator = mediator;

        public async Task StartClient()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(_url)
                .Build();

            _connection.On<string>("ReceiveSymmetricKey", (base64SymetricKey) =>
            {
                _symmetricKey = Convert.FromBase64String(base64SymetricKey);
            });

            await _connection.StartAsync();

            await _connection.InvokeAsync("ActualizarClienteConectado", TipoConexionCliente.ApiATM, _tokenDocumentId);

            AgregarEventosHubConnection();
            OperacionesUsuario();

            // Esperar hasta que la clave simétrica haya sido establecida antes de enviar mensajes
            while (_symmetricKey == null)
            {
                await Task.Delay(100);
            }

            var message = "<Request>...</Request>";
            var encryptedMessage = _xmlEncryptionService.EncryptString(message, _symmetricKey);
            await _connection.InvokeAsync("SendMessage", encryptedMessage);
        }

        private void OperacionesUsuario()
        {
            _connection?.On<string>("OnRegistro", async (dataEncriptada) =>
            {
                var usuarioDto = JsonConvert.DeserializeObject<UsuarioDto>(dataEncriptada)!;
                try
                {
                    var response = await _mediator.Send(new RegistroCommand(usuarioDto));

                    var responseData = XmlEncryptionService.SerializeToXml(response);
                    var responseEncrypted = _xmlEncryptionService.EncryptString(responseData);
                    await _connection.InvokeAsync("ReceiveMessage", responseEncrypted);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }
            });

            //_connection?.On<string>("OnLogin", async (dataEncriptada) =>
            //{
            //    var dataDesencriptada = _xmlEncryptionService.DecryptString(dataEncriptada, _symmetricKey);
            //    var usuarioDto = XmlEncryptionService.DeserializeFromXml<UsuarioDto>(dataDesencriptada);
            //    using var usuariosService = _serviceProvider.GetRequiredService<IUsuariosService>();
            //    var response = await usuariosService.LoginAsync(usuarioDto);

            //    // Enviar la respuesta de vuelta al servidor, si es necesario
            //    var responseData = XmlEncryptionService.SerializeToXml(response);
            //    var responseEncrypted = _xmlEncryptionService.EncryptString(responseData, _symmetricKey);
            //    await _connection.InvokeAsync("ReceiveMessage", responseEncrypted);
            //});
        }

        public void AgregarEventosHubConnection()
        {
            if (_connection == null)
            {
                return;
            }
            _connection.Reconnecting += error =>
            {
                Console.WriteLine("Reconecting..");
                return Task.CompletedTask;
            };

            _connection.Reconnected += connectionId =>
            {
                Console.WriteLine("Client reconected..");
                _connection.InvokeAsync("ActualizarClienteConectado", TipoConexionCliente.ApiATM, _tokenDocumentId);

                return Task.CompletedTask;
            };

            _connection.Closed += async error =>
            {
                Console.WriteLine("connection closed");
                try
                {
                    await StartClient();
                }
                catch (Exception)
                {
                    await Task.Delay(10000);
                    await StartClient();
                }
            };
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            Console.WriteLine("DisposeAsync SignalRClient");

            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("StartAsync SignalRClient");

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
