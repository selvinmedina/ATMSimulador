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
}
