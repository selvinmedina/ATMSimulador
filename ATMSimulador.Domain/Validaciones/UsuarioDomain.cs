using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Security;

namespace ATMSimulador.Domain.Validations
{
    public class UsuarioDomain
    {
        private readonly EncryptionService _encryptionService;
        public UsuarioDomain(EncryptionService encryptionService)
        {
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
        }

        public Response<Usuario> CreateUser(UsuarioDto usuarioDto)
        {
            if (string.IsNullOrWhiteSpace(usuarioDto.NombreUsuario))
                return Response<Usuario>.Fail(UsuariosMensajes.MSU_002);

            if (string.IsNullOrWhiteSpace(usuarioDto.Pin) || usuarioDto.Pin.Length != 4)
                return Response<Usuario>.Fail(UsuariosMensajes.MSU_003);

            var user = new Usuario()
            {
                NombreUsuario = usuarioDto.NombreUsuario,
                Pin = _encryptionService.EncryptBytes(usuarioDto.Pin)
            };

            return Response<Usuario>.Success(user);
        }

        public Response<bool> CheckLoginDto(UsuarioDto usuarioDto)
        {
            if (string.IsNullOrWhiteSpace(usuarioDto.NombreUsuario))
                return Response<bool>.Fail(UsuariosMensajes.MSU_002);

            if (string.IsNullOrWhiteSpace(usuarioDto.Pin))
                return Response<bool>.Fail(UsuariosMensajes.MSU_003);

            return Response<bool>.Success(true);
        }

        public bool VerifyPin(string enteredPin, byte[] storedHash)
        {
            var enteredHash = _encryptionService.DecryptBytes(enteredPin);
            return storedHash.SequenceEqual(enteredHash);
        }

    }
}
