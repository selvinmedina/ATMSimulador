using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Validations;
using ATMSimulador.Features.Auth;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace ATMSimulador.Features.Usuarios
{
    public class UsuariosService : IUsuariosService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UsuariosService> _logger;
        private readonly UsuarioDomain _usuarioDomain;
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsuariosService(
            ILogger<UsuariosService> logger,
            IUnitOfWork unitOfWork,
            UsuarioDomain usuarioDomain,
            IAuthService authService,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _usuarioDomain = usuarioDomain;
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
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

                // Registrar auditoría
                RegistrarAuditoria(usuarioDto.UsuarioId, "Registro de Usuario", $"Usuario {usuarioDto.NombreUsuario} registrado exitosamente.");

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

                // Registrar auditoría
                RegistrarAuditoria(user.UsuarioId, "Inicio de Sesión", $"Usuario {user.NombreUsuario} inició sesión exitosamente.");

                return Response<LoginRespuestaDto>.Success(respuesta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, UsuariosMensajes.MSU_005);
                return Response<LoginRespuestaDto>.Fail(ex.Message);
            }
        }

        public Response<UsuarioDataDto> GetUserDataAsync(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken == null)
            {
                return Response<UsuarioDataDto>.Fail("Invalid token.");
            }

            var userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == "userId");

            if (userIdClaim == null)
            {
                return Response<UsuarioDataDto>.Fail("User ID not found in the token.");
            }

            var userId = userIdClaim.Value;

            // Registrar auditoría
            RegistrarAuditoria(int.Parse(userId), "Obtención de Datos de Usuario", $"Datos del usuario {userId} obtenidos exitosamente.");

            return Response<UsuarioDataDto>.Success(new UsuarioDataDto { UserId = userId });
        }

        private void RegistrarAuditoria(int usuarioId, string tipoActividad, string descripcion)
        {
            var auditoria = new Auditoria
            {
                UsuarioId = usuarioId,
                TipoActividad = tipoActividad,
                FechaActividad = DateTime.UtcNow,
                Descripcion = descripcion
            };

            _unitOfWork.Repository<Auditoria>().Add(auditoria);
            _unitOfWork.SaveAsync().Wait();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
