namespace ATMSimulador.Domain.Entities
{
    public class Cuenta
    {
        public int CuentaId { get; set; }
        public int UsuarioId { get; set; }
        public string NumeroCuenta { get; set; } = null!;
        public decimal Saldo { get; set; }
        public bool Activa { get; set; }

        public Usuario Usuario { get; set; } = null!;
        public ICollection<Transaccion> Transacciones { get; set; } = new List<Transaccion>();
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    }
}
