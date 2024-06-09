using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using System.ServiceModel;

namespace ATMSimulador.Features.Usuarios
{
    [ServiceContract(Namespace = "http://atm.com/service/")]
    public interface IUsuariosService : IDisposable
    {
        [OperationContract(Name = "Login")]
        Task<Response<LoginRespuestaDto>> LoginAsync(UsuarioDto usuarioDto);

        [OperationContract(Name = "Registro")]
        Task<Response<UsuarioDto>> RegistroAsync(UsuarioDto usuarioDto);

        [OperationContract(Name = "GetUserData")]
        Response<UsuarioDataDto> GetUserDataAsync(string token);
    }
}
