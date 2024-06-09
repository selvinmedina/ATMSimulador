using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Security;

namespace ATMSimulador.Domain.Validaciones
{
    public class CuentaDomain
    {
        private readonly EncryptionService _encryptionService;

        public CuentaDomain(EncryptionService encryptionService)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        }

        public Response<Cuenta> CreateCuenta(CuentaDto cuentaDto)
        {
            if (string.IsNullOrWhiteSpace(cuentaDto.NumeroCuenta))
                return Response<Cuenta>.Fail(CuentasMensajes.MSC_002);

            var cuenta = new Cuenta
            {
                UsuarioId = cuentaDto.UsuarioId,
                NumeroCuenta = cuentaDto.NumeroCuenta,
                Saldo = _encryptionService.EncryptBytes(cuentaDto.Saldo.ToString("F2")),
                Activa = cuentaDto.Activa
            };

            return Response<Cuenta>.Success(cuenta);
        }

        public Response<bool> ValidateTransferencia(Cuenta cuentaOrigen, Cuenta cuentaDestino, decimal monto)
        {
            decimal saldoOrigen = decimal.Parse(_encryptionService.Decrypt(cuentaOrigen.Saldo));

            if (saldoOrigen < monto)
                return Response<bool>.Fail(CuentasMensajes.MSC_003);

            return Response<bool>.Success(true);
        }

        public decimal DecryptSaldo(byte[] encryptedSaldo)
        {
            var saldoString = _encryptionService.Decrypt(encryptedSaldo);
            return decimal.Parse(saldoString);
        }

        public byte[] EncryptSaldo(decimal saldo)
        {
            var saldoString = saldo.ToString("F2");
            return _encryptionService.EncryptBytes(saldoString);
        }
    }
}
