using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;

namespace ATMSimulador.Features.Usuarios
{
    public interface IUsuariosService : IDisposable
    {
        Task<Response<UsuarioDto>> LoginAsync(UsuarioDto usuarioDto);
        Task<Response<UsuarioDto>> Registro(UsuarioDto usuarioDto);
    }
}