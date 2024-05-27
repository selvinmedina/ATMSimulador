using System;

namespace ATMSimulador.Dominio.Entities
{
    public class Auditoria
    {
        public int AuditoriaId { get; set; }
        public int UsuarioId { get; set; }
        public string TipoActividad { get; set; } = null!;
        public DateTime FechaActividad { get; set; }
        public string? Descripcion { get; set; }

        public Usuario Usuario { get; set; } = null!;

    }
}
