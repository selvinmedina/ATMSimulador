using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using System.ServiceModel;

namespace ATMSimulador.Features.Transacciones
{
    [ServiceContract(Namespace = "http://atm.com/service/")]
    public interface ITransaccionesService : IDisposable
    {
        [OperationContract(Name = "ListarTransacciones")]
        Task<Response<List<TransaccionDto>>> ListarTransaccionesAsync(int cuentaId);
    }
}
