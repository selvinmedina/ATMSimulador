using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Security;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ATMSimulador.Features.Servicios
{
    public class ServiciosService : IServiciosService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ServiciosService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EncryptionHelper _encryptionHelper;

        public ServiciosService(
            ILogger<ServiciosService> logger,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            EncryptionHelper encryptionHelper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
            _encryptionHelper = encryptionHelper;
        }

        public async Task<Response<ServicioDtoString>> CrearServicioAsync(ServicioDto servicioDto)
        {
            var userId = _httpContextAccessor!.HttpContext!.Items["userId"]!.ToString();
            if (!int.TryParse(userId, out int usuarioId))
            {
                return Response<ServicioDtoString>.Fail("Invalid user ID");
            }
            var servicio = new Servicio
            {
                NombreServicio = servicioDto.NombreServicio,
                Descripcion = servicioDto.Descripcion
            };

            try
            {
                _unitOfWork.Repository<Servicio>().Add(servicio);
                await _unitOfWork.SaveAsync();

                servicioDto.ServicioId = servicio.ServicioId;

                RegistrarAuditoria(usuarioId, "Crear Servicio", $"Servicio {servicioDto.NombreServicio} creado.");

                var encryptedServicioDto = _encryptionHelper.EncriptarPropiedades<ServicioDto, ServicioDtoString>(servicioDto);

                return Response<ServicioDtoString>.Success(encryptedServicioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando servicio");
                return Response<ServicioDtoString>.Fail(ex.Message);
            }
        }

        public async Task<Response<ServicioDtoString>> EditarServicioAsync(ServicioDto servicioDto)
        {
            var userId = _httpContextAccessor!.HttpContext!.Items["userId"]!.ToString();
            if (!int.TryParse(userId, out int usuarioId))
            {
                return Response<ServicioDtoString>.Fail("Invalid user ID");
            }

            var servicio = await _unitOfWork.Repository<Servicio>().AsQueryable().FirstOrDefaultAsync(x => x.ServicioId == servicioDto.ServicioId);

            if (servicio == null)
            {
                return Response<ServicioDtoString>.Fail("Servicio no encontrado");
            }

            servicio.NombreServicio = servicioDto.NombreServicio;
            servicio.Descripcion = servicioDto.Descripcion;

            try
            {
                _unitOfWork.Repository<Servicio>().Update(servicio);
                await _unitOfWork.SaveAsync();

                RegistrarAuditoria(usuarioId, "Editar Servicio", $"Servicio {servicioDto.NombreServicio} editado.");

                var encryptedServicioDto = _encryptionHelper.EncriptarPropiedades<ServicioDto, ServicioDtoString>(servicioDto);

                return Response<ServicioDtoString>.Success(encryptedServicioDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editando servicio");
                return Response<ServicioDtoString>.Fail(ex.Message);
            }
        }

        public async Task<Response<List<ServicioDtoString>>> ListarServiciosAsync()
        {
            try
            {
                var servicios = await _unitOfWork.Repository<Servicio>().AsQueryable().ToListAsync();
                var serviciosDto = servicios.Select(s => new ServicioDto
                {
                    ServicioId = s.ServicioId,
                    NombreServicio = s.NombreServicio,
                    Descripcion = s.Descripcion
                }).ToList();

                var encryptedServiciosDto = serviciosDto
                    .Select(s => _encryptionHelper.EncriptarPropiedades<ServicioDto, ServicioDtoString>(s))
                    .ToList();

                return Response<List<ServicioDtoString>>.Success(encryptedServiciosDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando servicios");
                return Response<List<ServicioDtoString>>.Fail(ex.Message);
            }
        }

        public async Task<Response<ServicioDtoString>> ListarServicioPorIdAsync(int servicioId)
        {
            var desencryptedServicioId = _httpContextAccessor.HttpContext?.Items["servicioId"]?.ToString();
            if (!int.TryParse(desencryptedServicioId, out servicioId))
            {
                return Response<ServicioDtoString>.Fail("Invalid service ID");
            }
            var servicio = await _unitOfWork.Repository<Servicio>().AsQueryable().FirstOrDefaultAsync(x => x.ServicioId == servicioId);

            if (servicio == null)
            {
                return Response<ServicioDtoString>.Fail("Servicio no encontrado");
            }

            var servicioDto = new ServicioDto
            {
                ServicioId = servicio.ServicioId,
                NombreServicio = servicio.NombreServicio,
                Descripcion = servicio.Descripcion
            };

            var encryptedServicioDto = _encryptionHelper.EncriptarPropiedades<ServicioDto, ServicioDtoString>(servicioDto);

            return Response<ServicioDtoString>.Success(encryptedServicioDto);
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
            _unitOfWork.SaveAsync(); // Guarda la auditoría en la base de datos
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
