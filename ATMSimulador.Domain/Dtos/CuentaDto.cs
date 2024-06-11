namespace ATMSimulador.Domain.Dtos
{
    public class CuentaDto
    {
        public int CuentaId { get; set; }
        public int UsuarioId { get; set; }
        public string NumeroCuenta { get; set; } = null!;
        public decimal Saldo { get; set; }
        public bool Activa { get; set; }
    }

    public class CuentaDtoString
    {
        public string CuentaId { get; set; } = null!;
        public string UsuarioId { get; set; } = null!;
        public string NumeroCuenta { get; set; } = null!;
        public string Saldo { get; set; } = null!;
        public string Activa { get; set; } = null!;
    }

}
