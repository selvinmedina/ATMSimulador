using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Validations;
using ATMSimulador.Features.Auth;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace ATMSimulador.Features.Usuarios
{
    public class UsuariosService : IUsuariosService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UsuariosService> _logger;
        private readonly UsuarioDomain _usuarioDomain;
        private readonly IAuthService _authService;

        public UsuariosService(
            ILogger<UsuariosService> logger,
            IUnitOfWork unitOfWork,
            UsuarioDomain usuarioDomain,
            IAuthService authService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _usuarioDomain = usuarioDomain;
            _authService = authService;
        }

        public async Task<Response<UsuarioDto>> RegistroAsync(UsuarioDto usuarioDto)
        {
            var validationResult = _usuarioDomain.CreateUser(usuarioDto);
            if (!validationResult.Ok)
            {
                return Response<UsuarioDto>.Fail(validationResult.Message);
            }

            try
            {
                _unitOfWork.Repository<Usuario>().Add(validationResult.Data!);
                await _unitOfWork.SaveAsync();

                usuarioDto.UsuarioId = validationResult.Data!.UsuarioId;

                return Response<UsuarioDto>.Success(usuarioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, UsuariosMensajes.MSU_001);
                return Response<UsuarioDto>.Fail(ex.Message);
            }
        }

        public async Task<Response<LoginRespuestaDto>> LoginAsync(UsuarioDto usuarioDto)
        {
            var isLoginDtoValid = _usuarioDomain.CheckLoginDto(usuarioDto);
            if (!isLoginDtoValid.Ok)
            {
                return Response<LoginRespuestaDto>.Fail(isLoginDtoValid.Message);
            }

            try
            {
                var user = await _unitOfWork.Repository<Usuario>().AsQueryable()
                    .FirstOrDefaultAsync(u => u.NombreUsuario == usuarioDto.NombreUsuario);

                if (user == null)
                {
                    return Response<LoginRespuestaDto>.Fail(UsuariosMensajes.MSU_004);
                }

                if (!_usuarioDomain.VerifyPin(usuarioDto.Pin, user.Pin))
                {
                    return Response<LoginRespuestaDto>.Fail(UsuariosMensajes.MSU_004);
                }

                var token = _authService.GenerateToken(user.UsuarioId, user.NombreUsuario);
                var tokenDescriptor = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var expires = tokenDescriptor.ValidTo;

                var tokenDto = new TokenDto
                {
                    AccessToken = token,
                    ExpiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds,
                    Exp = new DateTimeOffset(expires).ToUnixTimeSeconds()
                };

                var respuesta = new LoginRespuestaDto()
                {
                    access_token = tokenDto.AccessToken,
                    token_type = tokenDto.TokenType,
                    expires_in = tokenDto.ExpiresIn,
                    exp = tokenDto.Exp,
                    refresh_token = tokenDto.RefreshToken
                };

                return Response<LoginRespuestaDto>.Success(respuesta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, UsuariosMensajes.MSU_005);
                return Response<LoginRespuestaDto>.Fail(ex.Message);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
