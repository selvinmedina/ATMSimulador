using ATMSimulador.Domain;
using ATMSimulador.Domain.Dtos;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ATMSimulador.Features.Servicios
{
    [ServiceContract(Namespace = "http://atm.com/service/")]
    public interface IServiciosService : IDisposable
    {
        [OperationContract(Name = "CrearServicio")]
        Task<Response<ServicioDto>> CrearServicioAsync(ServicioDto servicioDto);

        [OperationContract(Name = "EditarServicio")]
        Task<Response<ServicioDto>> EditarServicioAsync(ServicioDto servicioDto);

        [OperationContract(Name = "ListarServicios")]
        Task<Response<List<ServicioDto>>> ListarServiciosAsync();

        [OperationContract(Name = "ListarServicioPorId")]
        Task<Response<ServicioDto>> ListarServicioPorIdAsync(int servicioId);
    }
}
