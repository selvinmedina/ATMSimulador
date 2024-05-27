using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using MediatR;

namespace ATMSimulador.Features.Usuarios.Mediator.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, Response<UsuarioDto>>
    {
        private readonly IUsuariosService _usuariosService;

        public LoginHandler(IUsuariosService usuariosService)
        {
            _usuariosService = usuariosService;
        }

        public async Task<Response<UsuarioDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            return await _usuariosService.LoginAsync(request.UsuarioDto);
        }
    }
}
