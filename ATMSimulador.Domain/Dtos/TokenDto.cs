namespace ATMSimulador.Domain.Dtos
{
    public class TokenDto
    {
        public string AccessToken { get; set; } = null!;

        public string TokenType { get; set; } = "Bearer";

        public int ExpiresIn { get; set; }

        public long Exp { get; set; }

        public string RefreshToken { get; set; } = null!;
    }

}
