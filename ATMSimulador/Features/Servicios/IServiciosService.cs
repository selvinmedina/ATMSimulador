using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using System.ServiceModel;

namespace ATMSimulador.Features.Servicios
{
    [ServiceContract(Namespace = "http://atm.com/service/")]
    public interface IServiciosService : IDisposable
    {
        [OperationContract(Name = "CrearServicio")]
        Task<Response<ServicioDtoString>> CrearServicioAsync(ServicioDto servicioDto);

        [OperationContract(Name = "EditarServicio")]
        Task<Response<ServicioDtoString>> EditarServicioAsync(ServicioDto servicioDto);

        [OperationContract(Name = "ListarServicios")]
        Task<Response<List<ServicioDtoString>>> ListarServiciosAsync();

        [OperationContract(Name = "ListarServicioPorId")]
        Task<Response<ServicioDtoString>> ListarServicioPorIdAsync(int servicioId);
    }
}
