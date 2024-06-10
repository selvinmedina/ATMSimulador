using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ATMSimulador.Features.Transacciones
{
    public class TransaccionesService(
        ILogger<TransaccionesService> logger,
        IUnitOfWork unitOfWork) : ITransaccionesService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<TransaccionesService> _logger = logger;

        public async Task<Response<List<TransaccionDto>>> ListarTransaccionesAsync(int cuentaId)
        {
            try
            {
                // TODO: Validar que se el usuario id el que esta listando las transacciones
                var transacciones = await _unitOfWork.Repository<Transaccion>()
                    .AsQueryable()
                    .Where(t => t.CuentaId == cuentaId)
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
                RegistrarAuditoria(cuentaId, "Listado de Transacciones", $"Listado de transacciones para la cuenta {cuentaId}");

                return Response<List<TransaccionDto>>.Success(transaccionesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando transacciones");
                return Response<List<TransaccionDto>>.Fail("Error listando transacciones");
            }
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

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
