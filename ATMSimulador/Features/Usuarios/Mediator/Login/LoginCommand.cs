using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using MediatR;

namespace ATMSimulador.Features.Usuarios.Mediator.Login
{
    public class LoginCommand : IRequest<Response<UsuarioDto>>
    {
        public UsuarioDto UsuarioDto { get; }

        public LoginCommand(UsuarioDto usuarioDto)
        {
            UsuarioDto = usuarioDto;
        }
    }
}
