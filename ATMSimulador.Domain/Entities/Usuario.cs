namespace ATMSimulador.Domain.Entities
{
    public class Usuario
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public byte[] HashContrasena { get; set; } = null!;
        public string Pin { get; set; } = null!;

        public ICollection<Cuenta> Cuentas { get; set; } = new List<Cuenta>();
        public ICollection<Auditoria> Auditorias { get; set; } = new List<Auditoria>();
    }
}
