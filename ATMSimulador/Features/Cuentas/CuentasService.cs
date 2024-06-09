using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Validaciones;
using EntityFramework.Infrastructure.Core.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace ATMSimulador.Features.Cuentas
{
    public class CuentasService : ICuentasService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CuentasService> _logger;
        private readonly CuentaDomain _cuentaDomain;

        public CuentasService(
            ILogger<CuentasService> logger,
            IUnitOfWork unitOfWork,
            CuentaDomain cuentaDomain)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _cuentaDomain = cuentaDomain;
        }

        public async Task<Response<decimal>> ConsultarSaldoAsync(int cuentaId)
        {
            var cuenta = await _unitOfWork.Repository<Cuenta>().AsQueryable().FirstOrDefaultAsync(x => x.CuentaId == cuentaId);
            if (cuenta == null)
            {
                return Response<decimal>.Fail(CuentasMensajes.MSC_004);
            }

            var saldo = _cuentaDomain.DecryptSaldo(cuenta.Saldo);
            return Response<decimal>.Success(saldo);
        }

        public async Task<Response<bool>> TransferirAsync(int cuentaOrigenId, int cuentaDestinoId, decimal monto)
        {
            var cuentaOrigen = await _unitOfWork.Repository<Cuenta>().AsQueryable().FirstOrDefaultAsync(x => x.CuentaId == cuentaOrigenId);
            var cuentaDestino = await _unitOfWork.Repository<Cuenta>().AsQueryable().FirstOrDefaultAsync(x => x.CuentaId == cuentaDestinoId);

            if (cuentaOrigen == null || cuentaDestino == null)
            {
                return Response<bool>.Fail(CuentasMensajes.MSC_004);
            }

            var validacion = _cuentaDomain.ValidateTransferencia(cuentaOrigen, cuentaDestino, monto);
            if (!validacion.Ok)
            {
                return Response<bool>.Fail(validacion.Message);
            }

            _unitOfWork.BeginTransaction();

            try
            {
                cuentaOrigen.Saldo = _cuentaDomain.EncryptSaldo(_cuentaDomain.DecryptSaldo(cuentaOrigen.Saldo) - monto);
                cuentaDestino.Saldo = _cuentaDomain.EncryptSaldo(_cuentaDomain.DecryptSaldo(cuentaDestino.Saldo) + monto);

                _unitOfWork.Repository<Cuenta>().Update(cuentaOrigen);
                _unitOfWork.Repository<Cuenta>().Update(cuentaDestino);
                await _unitOfWork.SaveAsync();

                _unitOfWork.Commit();
                return Response<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _unitOfWork.RollBack();
                _logger.LogError(ex, CuentasMensajes.MSC_005);
                return Response<bool>.Fail(CuentasMensajes.MSC_005);
            }
        }

        public async Task<Response<List<CuentaDto>>> ListarCuentasAsync(int usuarioId)
        {
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

            return Response<List<CuentaDto>>.Success(cuentasDto);
        }

        public async Task<Response<CuentaDto>> AperturarCuentaAsync(CuentaDto cuentaDto)
        {
            var cuentaResponse = _cuentaDomain.CreateCuenta(cuentaDto);

            if (!cuentaResponse.Ok)
            {
                return Response<CuentaDto>.Fail(cuentaResponse.Message);
            }

            var cuenta = cuentaResponse.Data!;

            try
            {
                _unitOfWork.Repository<Cuenta>().Add(cuenta);
                await _unitOfWork.SaveAsync();

                cuentaDto.CuentaId = cuenta.CuentaId;
                return Response<CuentaDto>.Success(cuentaDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, CuentasMensajes.MSC_001);
                return Response<CuentaDto>.Fail(ex.Message);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
