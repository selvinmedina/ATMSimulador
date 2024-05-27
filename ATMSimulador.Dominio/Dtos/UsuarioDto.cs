namespace ATMSimulador.Dominio.Dtos
{
    public class UsuarioDto
    {
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = null!;
        public string Pin { get; set; } = null!;
    }
}
