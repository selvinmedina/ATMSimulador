using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ATMSimulador.Features.Transacciones
{
    public class TransaccionesService(
        ILogger<TransaccionesService> logger,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor) : ITransaccionesService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<TransaccionesService> _logger = logger;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        private int ObtenerUsuarioId()
        {
            var userId = _httpContextAccessor!.HttpContext!.Items["userId"]!.ToString();
            int.TryParse(userId, out var usuarioId);

            return usuarioId;
        }

        public async Task<Response<List<TransaccionDto>>> ListarTransaccionesAsync(int cuentaId)
        {
            try
            {
                int usuarioId = ObtenerUsuarioId();
                var transacciones = await _unitOfWork.Repository<Transaccion>()
                    .AsQueryable()
                    .Include(t => t.Cuenta)
                    .Where(t => t.CuentaId == cuentaId && t.Cuenta.UsuarioId == usuarioId)
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

                // Registrar auditoría
                RegistrarAuditoria("Listado de Transacciones", $"Listado de transacciones para la cuenta {cuentaId}");

                return Response<List<TransaccionDto>>.Success(transaccionesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando transacciones");
                return Response<List<TransaccionDto>>.Fail("Error listando transacciones");
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
