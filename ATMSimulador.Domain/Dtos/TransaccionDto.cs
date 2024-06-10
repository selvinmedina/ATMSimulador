using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
