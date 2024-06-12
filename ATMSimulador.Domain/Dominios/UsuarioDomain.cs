using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Security;

namespace ATMSimulador.Domain.Dominios
{
    public class UsuarioDomain(EncryptionService encryptionService)
    {
        private readonly EncryptionService _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));

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
            if (usuarioDto == null)
                return Response<bool>.Fail(UsuariosMensajes.MSU_001);

            if (string.IsNullOrWhiteSpace(usuarioDto.NombreUsuario))
                return Response<bool>.Fail(UsuariosMensajes.MSU_002);

            if (string.IsNullOrWhiteSpace(usuarioDto.Pin))
                return Response<bool>.Fail(UsuariosMensajes.MSU_003);

            return Response<bool>.Success(true);
        }

        public bool VerifyPin(string enteredPin, byte[] storedHash)
        {
            var enteredHash = _encryptionService.EncryptBytes(enteredPin);
            return storedHash.SequenceEqual(enteredHash);
        }

        private byte[] EncryptPin(string pin)
        {
            return _encryptionService.EncryptBytes(pin);
        }

        public Response<Usuario> UpdatePin(Usuario usuario, string nuevoPin)
        {
            if (string.IsNullOrWhiteSpace(nuevoPin) || nuevoPin.Length != 4)
                return Response<Usuario>.Fail(UsuariosMensajes.MSU_003);

            usuario.Pin = EncryptPin(nuevoPin);
            return Response<Usuario>.Success(usuario);
        }

    }
}
