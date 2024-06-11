namespace ATMSimulador.Domain.Dtos
{
    public class UsuarioDto
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string Pin { get; set; } = null!;
    }

    public class UsuarioDtoString
    {
        public string UsuarioId { get; set; } = null!;
        public string NombreUsuario { get; set; } = null!;
        public string Pin { get; set; } = null!;
    }
}
