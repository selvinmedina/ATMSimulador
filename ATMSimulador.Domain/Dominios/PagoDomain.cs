using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Mensajes;

namespace ATMSimulador.Domain.Dominios
{
    public class PagoDomain
    {
        public Response<Pago> CreatePago(PagoDto pagoDto)
        {
            if (pagoDto.Monto <= 0)
                return Response<Pago>.Fail(PagosMensajes.MSP_001);

            var pago = new Pago
            {
                ServicioId = pagoDto.ServicioId,
                CuentaId = pagoDto.CuentaId,
                Monto = pagoDto.Monto,
                FechaPago = DateTime.Now
            };

            return Response<Pago>.Success(pago);
        }
    }
}
