using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Validations;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ATMSimulador.Features.Usuarios
{
    public class UsuariosService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UsuarioDomain _dominio;
        private readonly ILogger<UsuariosService> _logger;

        public UsuariosService(
            ILogger<UsuariosService> logger,
            IUnitOfWork unitOfWork,
            UsuarioDomain usuarioDomain)
        {
            _unitOfWork = unitOfWork;
            _dominio = usuarioDomain;
            _logger = logger;
        }

        public async Task<Response<UsuarioDto>> CreateAsync(UsuarioDto usuarioDto)
        {
            var validationResult = _dominio.CreateUser(usuarioDto);
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
            var isLoginDtoValid = _dominio.CheckLoginDto(usuarioDto);
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

                if (!_dominio.VerifyPin(usuarioDto.Pin, user.Pin))
                {
                    return Response<UsuarioDto>.Fail(UsuariosMensajes.MSU_004);
                }

                usuarioDto.UsuarioId = user.UsuarioId;

                return Response<UsuarioDto>.Success(usuarioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in user");
                return Response<UsuarioDto>.Fail(ex.Message);
            }
        }
    }
}
