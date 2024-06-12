using ATMSimulador.Domain;
using ATMSimulador.Domain.Dominios;
using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Security;
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
        private readonly EncryptionHelper _encryptionHelper;

        public UsuariosService(
            ILogger<UsuariosService> logger,
            IUnitOfWork unitOfWork,
            UsuarioDomain usuarioDomain,
            IAuthService authService,
            IHttpContextAccessor httpContextAccessor,
            EncryptionHelper encryptionHelper
        )
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _usuarioDomain = usuarioDomain;
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
            _encryptionHelper = encryptionHelper;
        }

        public async Task<Response<UsuarioDtoString>> RegistroAsync(UsuarioDto usuarioDto)
        {
            var validationResult = _usuarioDomain.CreateUser(usuarioDto);
            if (!validationResult.Ok)
            {
                return Response<UsuarioDtoString>.Fail(validationResult.Message);
            }

            try
            {
                _unitOfWork.Repository<Usuario>().Add(validationResult.Data!);
                await _unitOfWork.SaveAsync();

                usuarioDto.UsuarioId = validationResult.Data!.UsuarioId;

                // Registrar auditoría
                RegistrarAuditoria(usuarioDto.UsuarioId, "Registro de Usuario", $"Usuario {usuarioDto.NombreUsuario} registrado exitosamente.");

                var respuesta = Response<UsuarioDto>.Success(usuarioDto);
                var encryptedResponse = _encryptionHelper.EncriptarResponse<UsuarioDto, UsuarioDtoString>(respuesta);

                return encryptedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, UsuariosMensajes.MSU_001);
                return Response<UsuarioDtoString>.Fail(ex.Message);
            }
        }

        public async Task<Response<LoginRespuestaDtoString>> LoginAsync(UsuarioDto usuarioDto)
        {
            var isLoginDtoValid = _usuarioDomain.CheckLoginDto(usuarioDto);
            if (!isLoginDtoValid.Ok)
            {
                return Response<LoginRespuestaDtoString>.Fail(isLoginDtoValid.Message);
            }

            try
            {
                var user = await _unitOfWork.Repository<Usuario>().AsQueryable()
                    .FirstOrDefaultAsync(u => u.NombreUsuario == usuarioDto.NombreUsuario);

                if (user == null)
                {
                    return Response<LoginRespuestaDtoString>.Fail(UsuariosMensajes.MSU_004);
                }

                if (!_usuarioDomain.VerifyPin(usuarioDto.Pin, user.Pin))
                {
                    return Response<LoginRespuestaDtoString>.Fail(UsuariosMensajes.MSU_004);
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

                // Registrar auditoría
                RegistrarAuditoria(user.UsuarioId, "Inicio de Sesión", $"Usuario {user.NombreUsuario} inició sesión exitosamente.");

                var respuesta = Response<LoginRespuestaDto>.Success(new LoginRespuestaDto()
                {
                    access_token = tokenDto.AccessToken,
                    token_type = tokenDto.TokenType,
                    expires_in = tokenDto.ExpiresIn,
                    exp = tokenDto.Exp,
                    refresh_token = tokenDto.RefreshToken
                });

                var encryptedResponse = _encryptionHelper.EncriptarResponse<LoginRespuestaDto, LoginRespuestaDtoString>(respuesta);

                return encryptedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, UsuariosMensajes.MSU_005);
                return Response<LoginRespuestaDtoString>.Fail(ex.Message);
            }
        }

        public Response<UsuarioDataDtoString> GetUserDataAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.Items["userId"]?.ToString();
            if (string.IsNullOrEmpty(userId))
            {
                return Response<UsuarioDataDtoString>.Fail("User ID not found");
            }

            var userData = new UsuarioDataDto { UserId = int.Parse(userId) };
            var respuesta = Response<UsuarioDataDto>.Success(userData);

            var encryptedResponse = _encryptionHelper.EncriptarResponse<UsuarioDataDto, UsuarioDataDtoString>(respuesta);

            return encryptedResponse;
        }

        public async Task<Response<bool>> CambiarPinAsync(string nuevoPin)
        {
            var usuarioId = ObtenerUsuarioId();
            if (usuarioId == 0)
            {
                return Response<bool>.Fail("Usuario no encontrado.");
            }

            try
            {
                var usuario = await _unitOfWork.Repository<Usuario>().AsQueryable()
                    .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId);

                if (usuario == null)
                {
                    return Response<bool>.Fail("Usuario no encontrado.");
                }

                var response = _usuarioDomain.UpdatePin(usuario, nuevoPin);
                if (!response.Ok)
                {
                    return Response<bool>.Fail(response.Message);
                }

                _unitOfWork.Repository<Usuario>().Update(response.Data!);
                await _unitOfWork.SaveAsync();

                RegistrarAuditoria(usuario.UsuarioId, "Cambio de Pin", $"El usuario {usuario.NombreUsuario} cambió su PIN exitosamente.");

                return Response<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar el PIN.");
                return Response<bool>.Fail("Error al cambiar el PIN.");
            }
        }

        private int ObtenerUsuarioId()
        {
            var userId = _httpContextAccessor!.HttpContext!.Items["userId"]!.ToString();
            int.TryParse(userId, out var usuarioId);
            return usuarioId;
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
            _unitOfWork.SaveAsync();
        }

        private bool _disposed = false; // Para detectar llamadas redundantes

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
