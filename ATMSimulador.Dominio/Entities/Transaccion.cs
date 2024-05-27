using System;

namespace ATMSimulador.Dominio.Entities
{
    public class Transaccion
    {
        public int TransaccionId { get; set; }
        public int CuentaId { get; set; }
        public string TipoTransaccion { get; set; } = null!;
        public decimal Monto { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public string Estado { get; set; } = null!;

        public Cuenta Cuenta { get; set; } = null!;
    }
}
