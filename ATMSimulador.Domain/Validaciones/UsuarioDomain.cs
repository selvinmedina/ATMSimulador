using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Security;

namespace ATMSimulador.Domain.Validations
{
    public class UsuarioDomain
    {
        private readonly EncryptionService _encryptionService;

        public UsuarioDomain(EncryptionService encryptionService)
        {
            _encryptionService = encryptionService;
        }

        public Response<Usuario> CreateUser(UsuarioDto usuarioDto)
        {
            if (string.IsNullOrWhiteSpace(usuarioDto.NombreUsuario))
                return Response<Usuario>.Fail("El nombre de usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(usuarioDto.Pin) || usuarioDto.Pin.Length != 4)
                return Response<Usuario>.Fail("El PIN es obligatorio y debe tener 4 dígitos.");

            var user = new Usuario()
            {
                NombreUsuario = usuarioDto.NombreUsuario,
                Pin = _encryptionService.ComputeMd5Hash(usuarioDto.Pin)
            };

            return Response<Usuario>.Success(user);
        }

        public Response<bool> CheckLoginDto(UsuarioDto usuarioDto)
        {
            if (string.IsNullOrWhiteSpace(usuarioDto.NombreUsuario))
                return Response<bool>.Fail("El nombre de usuario es obligatorio.");

            if (string.IsNullOrWhiteSpace(usuarioDto.Pin))
                return Response<bool>.Fail("El PIN es obligatorio.");

            return Response<bool>.Success(true);
        }

        public bool VerifyPin(string enteredPin, string storedHash)
        {
            var enteredHash = _encryptionService.ComputeMd5Hash(enteredPin);
            return storedHash == enteredHash;
        }
    }
}
