using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using System.ServiceModel;
using System.Threading.Tasks;

namespace ATMSimulador.Features.Usuarios
{
    [ServiceContract]
    public interface IUsuariosService : IDisposable
    {
        [OperationContract]
        Task<Response<LoginRespuestaDto>> LoginAsync(UsuarioDto usuarioDto);

        [OperationContract]
        Task<Response<UsuarioDto>> RegistroAsync(UsuarioDto usuarioDto);
    }
}
