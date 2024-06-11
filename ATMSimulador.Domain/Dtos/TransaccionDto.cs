namespace ATMSimulador.Domain.Dtos
{
    public class TransaccionDto
    {
        public int TransaccionId { get; set; }
        public int CuentaId { get; set; }
        public string TipoTransaccion { get; set; } = null!;
        public decimal Monto { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public string Estado { get; set; } = null!;
    }

    public class TransaccionDtoString
    {
        public string TransaccionId { get; set; } = null!;
        public string CuentaId { get; set; } = null!;
        public string TipoTransaccion { get; set; } = null!;
        public string Monto { get; set; } = null!;
        public string FechaTransaccion { get; set; } = null!;
        public string Estado { get; set; } = null!;
    }

}
