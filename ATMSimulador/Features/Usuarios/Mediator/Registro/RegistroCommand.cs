using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using MediatR;

namespace ATMSimulador.Features.Usuarios.Mediator.Registro
{
    public class RegistroCommand : IRequest<Response<UsuarioDto>>
    {
        public UsuarioDto UsuarioDto { get; }

        public RegistroCommand(UsuarioDto usuarioDto)
        {
            UsuarioDto = usuarioDto;
        }
    }
}
