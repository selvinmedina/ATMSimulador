using System;

namespace ATMSimulador.Dominio.Entities
{
    public class Pago
    {
        public int PagoId { get; set; }
        public int ServicioId { get; set; }
        public int CuentaId { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }

        public Servicio Servicio { get; set; } = null!;
        public Cuenta Cuenta { get; set; } = null!;
    }
}
