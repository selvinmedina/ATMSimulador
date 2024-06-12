namespace ATMSimulador.Domain.Dtos
{
    public class PagoDto
    {
        public int ServicioId { get; set; }
        public int CuentaId { get; set; }
        public decimal Monto { get; set; }
    }

    public class PagoDtoString
    {
        public string ServicioId { get; set; } = null!;
        public string CuentaId { get; set; } = null!;
        public string Monto { get; set; } = null!;
    }
}
