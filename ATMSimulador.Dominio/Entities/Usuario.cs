using System.Collections.Generic;

namespace ATMSimulador.Dominio.Entities
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public byte[] Pin { get; set; } = null!;

        public ICollection<Cuenta> Cuentas { get; set; } = [];
        public ICollection<Auditoria> Auditorias { get; set; } = [];
    }
}
