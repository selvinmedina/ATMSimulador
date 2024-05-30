using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Validations;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ATMSimulador.Features.Usuarios
{
    public class UsuariosService(
        ILogger<UsuariosService> logger,
        IUnitOfWork unitOfWork,
        UsuarioDomain usuarioDomain) : IUsuariosService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<UsuariosService> _logger = logger;
        private readonly UsuarioDomain _usuarioDomain = usuarioDomain;

        // TODO: Pendiente de agregar con signalR esto
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

        public async Task<Response<UsuarioDto>> LoginAsync(UsuarioDto usuarioDto)
        {
            var isLoginDtoValid = _usuarioDomain.CheckLoginDto(usuarioDto);
            if (!isLoginDtoValid.Ok)
            {
                return Response<UsuarioDto>.Fail(isLoginDtoValid.Message);
            }

            try
            {
                var user = await _unitOfWork.Repository<Usuario>().AsQueryable()
                    .FirstOrDefaultAsync(u => u.NombreUsuario == usuarioDto.NombreUsuario);

                if (user == null)
                {
                    return Response<UsuarioDto>.Fail(UsuariosMensajes.MSU_004);
                }

                if (!_usuarioDomain.VerifyPin(usuarioDto.Pin, user.Pin))
                {
                    return Response<UsuarioDto>.Fail(UsuariosMensajes.MSU_004);
                }

                usuarioDto.UsuarioId = user.UsuarioId;

                return Response<UsuarioDto>.Success(usuarioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, UsuariosMensajes.MSU_005);
                return Response<UsuarioDto>.Fail(ex.Message);
            }
        }


        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
