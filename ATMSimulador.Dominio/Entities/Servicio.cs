using System.Collections.Generic;

namespace ATMSimulador.Dominio.Entities
{
    public class Servicio
    {
        public int ServicioId { get; set; }
        public string NombreServicio { get; set; } = null!;
        public string? Descripcion { get; set; }

        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }
}
