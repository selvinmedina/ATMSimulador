using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using System.ServiceModel;

namespace ATMSimulador.Features.Cuentas
{
    [ServiceContract(Namespace = "http://atm.com/service/")]
    public interface ICuentasService : IDisposable
    {
        [OperationContract(Name = "ConsultarSaldo")]
        Task<Response<string>> ConsultarSaldoAsync(int cuentaId);

        [OperationContract(Name = "Transferir")]
        Task<Response<bool>> TransferirAsync(int cuentaOrigenId, int cuentaDestinoId, decimal monto);

        [OperationContract(Name = "ListarCuentas")]
        Task<Response<List<CuentaDtoString>>> ListarCuentasAsync();

        [OperationContract(Name = "AperturarCuenta")]
        Task<Response<CuentaDtoString>> AperturarCuentaAsync(CuentaDto cuentaDto);

        [OperationContract(Name = "Retirar")]
        Task<Response<RetiroDtoString>> RetirarAsync(int cuentaId, decimal monto);

        [OperationContract(Name = "Depositar")]
        Task<Response<DepositoDtoString>> DepositarAsync(int cuentaId, decimal monto);
    }
}
