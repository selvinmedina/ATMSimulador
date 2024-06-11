using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using System.ServiceModel;

namespace ATMSimulador.Features.Usuarios
{
    [ServiceContract(Namespace = "http://atm.com/service/")]
    public interface IUsuariosService : IDisposable
    {
        [OperationContract(Name = "Login")]
        Task<Response<LoginRespuestaDtoString>> LoginAsync(UsuarioDto usuarioDto);

        [OperationContract(Name = "Registro")]
        Task<Response<UsuarioDtoString>> RegistroAsync(UsuarioDto usuarioDto);

        [OperationContract(Name = "GetUserData")]
        Response<UsuarioDataDtoString> GetUserDataAsync();
    }
}
