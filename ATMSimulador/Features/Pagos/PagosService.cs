using ATMSimulador.Domain;
using ATMSimulador.Domain.Dominios;
using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Transacciones;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ATMSimulador.Features.Pagos
{
    public class PagosService(
        ILogger<PagosService> logger,
        IUnitOfWork unitOfWork,
        PagoDomain pagoDomain,
        CuentaDomain cuentaDomain,
        IHttpContextAccessor httpContextAccessor) : IPagosService
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<PagosService> _logger = logger;
        private readonly PagoDomain _pagoDomain = pagoDomain;
        private readonly CuentaDomain _cuentaDomain = cuentaDomain;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        private int ObtenerUsuarioId()
        {
            var userId = _httpContextAccessor!.HttpContext!.Items["userId"]!.ToString();
            int.TryParse(userId, out var usuarioId);

            return usuarioId;
        }

        public async Task<Response<PagoDto>> RealizarPagoAsync(PagoDto pagoDto)
        {
            var usuarioId = ObtenerUsuarioId();
            var cuenta = await _unitOfWork.Repository<Cuenta>().AsQueryable()
                        .FirstOrDefaultAsync(x => x.CuentaId == pagoDto.CuentaId && x.UsuarioId == usuarioId);

            if (cuenta == null)
            {
                return Response<PagoDto>.Fail(CuentasMensajes.MSC_004);
            }

            var validacion = _cuentaDomain.ValidateSaldo(cuenta, pagoDto.Monto);
            if (!validacion.Ok)
            {
                return Response<PagoDto>.Fail(validacion.Message);
            }

            _unitOfWork.BeginTransaction();

            try
            {
                var saldoActual = _cuentaDomain.DecryptSaldo(cuenta.Saldo);
                var nuevoSaldo = saldoActual - pagoDto.Monto;
                cuenta.Saldo = _cuentaDomain.EncryptSaldo(nuevoSaldo);

                _unitOfWork.Repository<Cuenta>().Update(cuenta);

                var pagoResponse = _pagoDomain.CreatePago(pagoDto);
                if (!pagoResponse.Ok)
                {
                    return Response<PagoDto>.Fail(pagoResponse.Message);
                }

                var pago = pagoResponse.Data!;
                _unitOfWork.Repository<Pago>().Add(pago);

                // Registrar transacción de pago
                var servicio = await _unitOfWork.Repository<Servicio>().AsQueryable().FirstOrDefaultAsync(x => x.ServicioId == pagoDto.ServicioId);
                if (servicio == null)
                {
                    return Response<PagoDto>.Fail("Servicio no encontrado");
                }

                var transaccionPago = new Transaccion
                {
                    CuentaId = pagoDto.CuentaId,
                    TipoTransaccion = string.Format(TransaccionTipos.PAGO_SERVICIO, servicio.NombreServicio),
                    Monto = pagoDto.Monto,
                    FechaTransaccion = DateTime.UtcNow,
                    Estado = "COMPLETADO"
                };

                _unitOfWork.Repository<Transaccion>().Add(transaccionPago);
                await _unitOfWork.SaveAsync();
                _unitOfWork.Commit();

                RegistrarAuditoria(cuenta.UsuarioId, "Pago de Servicio", $"Pago de {pagoDto.Monto} para el servicio {servicio.NombreServicio}");

                return Response<PagoDto>.Success(pagoDto);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                _logger.LogError(ex, PagosMensajes.MSP_002);
                return Response<PagoDto>.Fail(PagosMensajes.MSP_002);
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
