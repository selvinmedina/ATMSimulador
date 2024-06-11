using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Security;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ATMSimulador.Features.Transacciones
{
    public class TransaccionesService : ITransaccionesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TransaccionesService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EncryptionHelper _encryptionHelper;

        public TransaccionesService(
            ILogger<TransaccionesService> logger,
            IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            EncryptionHelper encryptionHelper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _encryptionHelper = encryptionHelper;
        }

        private int ObtenerUsuarioId()
        {
            var userId = _httpContextAccessor.HttpContext!.Items["userId"]?.ToString();
            int.TryParse(userId, out var usuarioId);
            return usuarioId;
        }

        public async Task<Response<List<TransaccionDtoString>>> ListarTransaccionesAsync()
        {
            try
            {
                int usuarioId = ObtenerUsuarioId();
                var transacciones = await _unitOfWork.Repository<Transaccion>()
                    .AsQueryable()
                    .Include(t => t.Cuenta)
                    .Where(t => t.Cuenta.UsuarioId == usuarioId)
                    .ToListAsync();

                var transaccionesDto = transacciones.Select(t => new TransaccionDto
                {
                    TransaccionId = t.TransaccionId,
                    CuentaId = t.CuentaId,
                    TipoTransaccion = t.TipoTransaccion,
                    Monto = t.Monto,
                    FechaTransaccion = t.FechaTransaccion,
                    Estado = t.Estado
                }).ToList();

                if (!transaccionesDto.Any())
                {
                    return Response<List<TransaccionDtoString>>.Fail("No se encontraron transacciones");
                }

                var transaccionesDtoString = transaccionesDto
                    .Select(t => _encryptionHelper.EncriptarPropiedades<TransaccionDto, TransaccionDtoString>(t))
                    .ToList();

                var encryptedResponse = Response<List<TransaccionDtoString>>.Success(transaccionesDtoString);

                // Registrar auditoría
                RegistrarAuditoria("Listado de Transacciones", $"Listado de transacciones para el usuario {usuarioId}");

                return encryptedResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando transacciones");
                return Response<List<TransaccionDtoString>>.Fail("Error listando transacciones");
            }
        }

        private void RegistrarAuditoria(string tipoActividad, string descripcion)
        {
            int usuarioId = ObtenerUsuarioId();
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
