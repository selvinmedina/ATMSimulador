using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMSimulador.Domain.Dtos
{
    public class RetiroDto
    {
        public decimal MontoRetirado { get; set; }
        public decimal SaldoActual { get; set; }
        public string NumeroCuenta { get; set; } = null!;
        public DateTime FechaTransaccion { get; set; }
    }

    public class RetiroDtoString
    {
        public string MontoRetirado { get; set; } = null!;
        public string SaldoActual { get; set; } = null!;
        public string NumeroCuenta { get; set; } = null!;
        public string FechaTransaccion { get; set; } = null!;
    }

}
