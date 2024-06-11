namespace ATMSimulador.Domain.Dtos
{
    public class LoginRespuestaDto
    {
        public string access_token { get; set; } = null!;
        public string token_type { get; set; } = null!;
        public int expires_in { get; set; }
        public long exp { get; set; }
        public string refresh_token { get; set; } = null!;
    }

    public class LoginRespuestaDtoString
    {
        public string access_token { get; set; } = null!;
        public string token_type { get; set; } = null!;
        public string expires_in { get; set; } = null!;
        public string exp { get; set; } = null!;
        public string refresh_token { get; set; } = null!;
    }
}
