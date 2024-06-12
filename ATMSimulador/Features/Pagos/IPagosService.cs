using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using System.ServiceModel;

namespace ATMSimulador.Features.Pagos
{
    [ServiceContract(Namespace = "http://atm.com/service/")]
    public interface IPagosService : IDisposable
    {
        [OperationContract(Name = "RealizarPago")]
        Task<Response<PagoDtoString>> RealizarPagoAsync(PagoDto pagoDto);
    }
}
