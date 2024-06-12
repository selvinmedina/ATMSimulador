using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMSimulador.Domain.Dtos
{
    public class DepositoDto
    {
        public int CuentaId { get; set; }
        public decimal Monto { get; set; }
        public decimal SaldoActual { get; set; }
        public string NumeroCuenta { get; set; } = null!;
        public DateTime FechaTransaccion { get; set; }
    }

    public class DepositoDtoString
    {
        public string CuentaId { get; set; } = null!;
        public string Monto { get; set; } = null!;
        public string SaldoActual { get; set; } = null!;
        public string NumeroCuenta { get; set; } = null!;
        public string FechaTransaccion { get; set; } = null!;
    }
}
