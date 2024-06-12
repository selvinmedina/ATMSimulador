using ATMSimulador.Domain;
using ATMSimulador.Domain.Dominios;
using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Security;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using ATMSimulador.Domain.Transacciones;
using System.Text;

namespace ATMSimulador.Features.Cuentas
{
    public class CuentasService : ICuentasService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CuentasService> _logger;
        private readonly CuentaDomain _cuentaDomain;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly EncryptionHelper _encryptionHelper;

        public CuentasService(
            ILogger<CuentasService> logger,
            IUnitOfWork unitOfWork,
            CuentaDomain cuentaDomain,
            IHttpContextAccessor httpContextAccessor,
            EncryptionHelper encryptionHelper)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cuentaDomain = cuentaDomain;
            _httpContextAccessor = httpContextAccessor;
            _encryptionHelper = encryptionHelper;
        }

        public async Task<Response<string>> ConsultarSaldoAsync(int cuentaId)
        {
            int usuarioId = ObtenerUsuarioId();
            var cuenta = await _unitOfWork.Repository<Cuenta>().AsQueryable().FirstOrDefaultAsync(x => x.CuentaId == cuentaId && x.UsuarioId == usuarioId);
            if (cuenta == null)
            {
                return Response<string>.Fail(CuentasMensajes.MSC_004);
            }

            var saldo = _cuentaDomain.DecryptSaldo(cuenta.Saldo);
            var encryptedSaldo = _encryptionHelper.EncryptionService.Encrypt(saldo.ToString());
            RegistrarAuditoria(cuenta.UsuarioId, "Consulta de Saldo", $"Consulta de saldo para la cuenta {cuenta.NumeroCuenta}");
            return Response<string>.Success(encryptedSaldo);
        }

        public async Task<Response<bool>> TransferirAsync(int cuentaOrigenId, int cuentaDestinoId, decimal monto)
        {
            int usuarioId = ObtenerUsuarioId();

            var cuentaOrigen = await _unitOfWork.Repository<Cuenta>().AsQueryable().FirstOrDefaultAsync(x => x.CuentaId == cuentaOrigenId && x.UsuarioId == usuarioId);
            var cuentaDestino = await _unitOfWork.Repository<Cuenta>().AsQueryable().FirstOrDefaultAsync(x => x.CuentaId == cuentaDestinoId);

            if (cuentaOrigen == null || cuentaDestino == null)
            {
                return Response<bool>.Fail(CuentasMensajes.MSC_004);
            }

            var validacion = _cuentaDomain.ValidateTransferencia(cuentaOrigen, monto);
            if (!validacion.Ok)
            {
                return Response<bool>.Fail(validacion.Message);
            }

            _unitOfWork.BeginTransaction();

            try
            {
                var saldoOrigen = _cuentaDomain.DecryptSaldo(cuentaOrigen.Saldo) - monto;
                var saldoDestino = _cuentaDomain.DecryptSaldo(cuentaDestino.Saldo) + monto;

                cuentaOrigen.Saldo = _cuentaDomain.EncryptSaldo(saldoOrigen);
                cuentaDestino.Saldo = _cuentaDomain.EncryptSaldo(saldoDestino);

                _unitOfWork.Repository<Cuenta>().Update(cuentaOrigen);
                _unitOfWork.Repository<Cuenta>().Update(cuentaDestino);

                // Registrar transacción para la cuenta de origen
                var transaccionRetiro = new Transaccion
                {
                    CuentaId = cuentaOrigenId,
                    TipoTransaccion = string.Format(TransaccionTipos.TEF_A, cuentaDestino.NumeroCuenta),
                    Monto = monto,
                    FechaTransaccion = DateTime.UtcNow,
                    Estado = "COMPLETADO"
                };
                _unitOfWork.Repository<Transaccion>().Add(transaccionRetiro);

                // Registrar transacción para la cuenta de destino
                var transaccionDeposito = new Transaccion
                {
                    CuentaId = cuentaDestinoId,
                    TipoTransaccion = string.Format(TransaccionTipos.TEF_DE, cuentaOrigen.NumeroCuenta),
                    Monto = monto,
                    FechaTransaccion = DateTime.UtcNow,
                    Estado = "COMPLETADO"
                };
                _unitOfWork.Repository<Transaccion>().Add(transaccionDeposito);

                await _unitOfWork.SaveAsync();
                _unitOfWork.Commit();

                RegistrarAuditoria(cuentaOrigen.UsuarioId, "Transferencia", $"Transferencia de {monto} desde la cuenta {cuentaOrigen.NumeroCuenta} a la cuenta {cuentaDestino.NumeroCuenta}");
                RegistrarAuditoria(cuentaDestino.UsuarioId, "Transferencia Recibida", $"Transferencia recibida de {monto} desde la cuenta {cuentaOrigen.NumeroCuenta}");

                return Response<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                _logger.LogError(ex, CuentasMensajes.MSC_005);
                return Response<bool>.Fail(CuentasMensajes.MSC_005);
            }
        }

        public async Task<Response<List<CuentaDtoString>>> ListarCuentasAsync()
        {
            int usuarioId = ObtenerUsuarioId();

            var cuentas = await _unitOfWork.Repository<Cuenta>()
                .AsQueryable()
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            var cuentasDto = cuentas.Select(c => new CuentaDto
            {
                CuentaId = c.CuentaId,
                UsuarioId = c.UsuarioId,
                NumeroCuenta = c.NumeroCuenta,
                Saldo = _cuentaDomain.DecryptSaldo(c.Saldo),
                Activa = c.Activa
            }).ToList();

            var encryptedCuentasDto = cuentasDto
                .Select(c => _encryptionHelper.EncriptarPropiedades<CuentaDto, CuentaDtoString>(c))
                .ToList();

            RegistrarAuditoria(usuarioId, "Listar Cuentas", "Listado de cuentas del usuario");

            return Response<List<CuentaDtoString>>.Success(encryptedCuentasDto);
        }

        public async Task<Response<CuentaDtoString>> AperturarCuentaAsync(CuentaDto cuentaDto)
        {
            int usuarioId = ObtenerUsuarioId();
            cuentaDto.UsuarioId = usuarioId;

            var cuentaResponse = _cuentaDomain.CreateCuenta(cuentaDto);

            if (!cuentaResponse.Ok)
            {
                return Response<CuentaDtoString>.Fail(cuentaResponse.Message);
            }

            var cuenta = cuentaResponse.Data!;

            try
            {
                _unitOfWork.Repository<Cuenta>().Add(cuenta);
                await _unitOfWork.SaveAsync(); // Guarda la cuenta en la base de datos

                // Registrar transacción de apertura
                var transaccionApertura = new Transaccion
                {
                    CuentaId = cuenta.CuentaId,
                    TipoTransaccion = TransaccionTipos.APERTURA,
                    Monto = cuentaDto.Saldo,
                    FechaTransaccion = DateTime.UtcNow,
                    Estado = "COMPLETADO"
                };

                _unitOfWork.Repository<Transaccion>().Add(transaccionApertura);
                await _unitOfWork.SaveAsync(); // Guarda la transacción en la base de datos

                cuentaDto.CuentaId = cuenta.CuentaId;

                var encryptedCuentaDto = _encryptionHelper.EncriptarPropiedades<CuentaDto, CuentaDtoString>(cuentaDto);

                RegistrarAuditoria(cuenta.UsuarioId, "Apertura de Cuenta", $"Apertura de cuenta {cuenta.NumeroCuenta} con saldo inicial {cuentaDto.Saldo}");

                return Response<CuentaDtoString>.Success(encryptedCuentaDto);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                _logger.LogError(ex, CuentasMensajes.MSC_001);
                return Response<CuentaDtoString>.Fail(ex.Message);
            }
        }

        public async Task<Response<RetiroDtoString>> RetirarAsync(int cuentaId, decimal monto)
        {
            int usuarioId = ObtenerUsuarioId();

            var cuenta = await _unitOfWork.Repository<Cuenta>()
                .AsQueryable()
                .FirstOrDefaultAsync(x => x.CuentaId == cuentaId && x.UsuarioId == usuarioId);

            if (cuenta == null)
            {
                return Response<RetiroDtoString>.Fail(CuentasMensajes.MSC_004);
            }

            var saldoActual = _cuentaDomain.DecryptSaldo(cuenta.Saldo);

            if (saldoActual < monto)
            {
                return Response<RetiroDtoString>.Fail(CuentasMensajes.MSC_006);
            }

            var nuevoSaldo = saldoActual - monto;
            cuenta.Saldo = _cuentaDomain.EncryptSaldo(nuevoSaldo);

            var transaccion = new Transaccion
            {
                CuentaId = cuentaId,
                TipoTransaccion = TransaccionTipos.RETIRO,
                Monto = monto,
                FechaTransaccion = DateTime.UtcNow,
                Estado = "COMPLETADO"
            };

            _unitOfWork.BeginTransaction();

            try
            {
                _unitOfWork.Repository<Cuenta>().Update(cuenta);
                _unitOfWork.Repository<Transaccion>().Add(transaccion);
                await _unitOfWork.SaveAsync();
                _unitOfWork.Commit();

                RegistrarAuditoria(cuenta.UsuarioId, "Retiro", $"Retiro de {monto} desde la cuenta {cuenta.NumeroCuenta}");

                var retiroDto = new RetiroDto
                {
                    MontoRetirado = monto,
                    SaldoActual = nuevoSaldo,
                    NumeroCuenta = cuenta.NumeroCuenta,
                    FechaTransaccion = transaccion.FechaTransaccion
                };

                var encryptedRetiroDto = _encryptionHelper.EncriptarPropiedades<RetiroDto, RetiroDtoString>(retiroDto);

                return Response<RetiroDtoString>.Success(encryptedRetiroDto);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                _logger.LogError(ex, CuentasMensajes.MSC_005);
                return Response<RetiroDtoString>.Fail(CuentasMensajes.MSC_005);
            }
        }

        public async Task<Response<DepositoDtoString>> DepositarAsync(int cuentaId, decimal monto)
        {
            int usuarioId = ObtenerUsuarioId();

            var cuenta = await _unitOfWork.Repository<Cuenta>().AsQueryable().FirstOrDefaultAsync(x => x.CuentaId == cuentaId && x.UsuarioId == usuarioId);

            if (cuenta == null)
            {
                return Response<DepositoDtoString>.Fail(CuentasMensajes.MSC_004);
            }

            _unitOfWork.BeginTransaction();

            try
            {
                var saldoActual = _cuentaDomain.DecryptSaldo(cuenta.Saldo) + monto;
                cuenta.Saldo = _cuentaDomain.EncryptSaldo(saldoActual);

                _unitOfWork.Repository<Cuenta>().Update(cuenta);

                // Registrar transacción de depósito
                var transaccionDeposito = new Transaccion
                {
                    CuentaId = cuentaId,
                    TipoTransaccion = TransaccionTipos.DEPOSITO,
                    Monto = monto,
                    FechaTransaccion = DateTime.UtcNow,
                    Estado = "COMPLETADO"
                };
                _unitOfWork.Repository<Transaccion>().Add(transaccionDeposito);

                await _unitOfWork.SaveAsync();
                _unitOfWork.Commit();

                var depositoDto = new DepositoDto
                {
                    CuentaId = cuenta.CuentaId,
                    Monto = monto,
                    SaldoActual = saldoActual,
                    NumeroCuenta = cuenta.NumeroCuenta,
                    FechaTransaccion = transaccionDeposito.FechaTransaccion
                };

                var encryptedDepositoDto = _encryptionHelper.EncriptarPropiedades<DepositoDto, DepositoDtoString>(depositoDto);

                RegistrarAuditoria(cuenta.UsuarioId, "Depósito", $"Depósito de {monto} a la cuenta {cuenta.NumeroCuenta}");

                return Response<DepositoDtoString>.Success(encryptedDepositoDto);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                _logger.LogError(ex, CuentasMensajes.MSC_005);
                return Response<DepositoDtoString>.Fail(CuentasMensajes.MSC_005);
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
