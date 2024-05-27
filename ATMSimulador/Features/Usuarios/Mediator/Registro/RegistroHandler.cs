using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using MediatR;

namespace ATMSimulador.Features.Usuarios.Mediator.Registro
{
    public class RegistroHandler : IRequestHandler<RegistroCommand, Response<UsuarioDto>>
    {
        private readonly IUsuariosService _usuariosService;

        public RegistroHandler(IUsuariosService usuariosService)
        {
            _usuariosService = usuariosService;
        }

        public async Task<Response<UsuarioDto>> Handle(RegistroCommand request, CancellationToken cancellationToken)
        {
            return await _usuariosService.RegistroAsync(request.UsuarioDto);
        }
    }
}
