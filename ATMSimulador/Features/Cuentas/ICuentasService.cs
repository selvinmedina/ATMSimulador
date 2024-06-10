using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using System.ServiceModel;

namespace ATMSimulador.Features.Cuentas
{
    [ServiceContract(Namespace = "http://atm.com/service/")]
    public interface ICuentasService : IDisposable
    {
        [OperationContract(Name = "ConsultarSaldo")]
        Task<Response<decimal>> ConsultarSaldoAsync(int cuentaId);

        [OperationContract(Name = "Transferir")]
        Task<Response<bool>> TransferirAsync(int cuentaOrigenId, int cuentaDestinoId, decimal monto);

        [OperationContract(Name = "ListarCuentas")]
        Task<Response<List<CuentaDto>>> ListarCuentasAsync();

        [OperationContract(Name = "AperturarCuenta")]
        Task<Response<CuentaDto>> AperturarCuentaAsync(CuentaDto cuentaDto);
    }
}
