namespace ATMSimulador.Domain.Dtos
{
    public class PagoDto
    {
        public int ServicioId { get; set; }
        public int CuentaId { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
    }
}
