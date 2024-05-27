namespace ATMSimulador.Features.Auth
{
    public interface IAuthService
    {
        string GenerateToken(int userId, string email);
    }
}