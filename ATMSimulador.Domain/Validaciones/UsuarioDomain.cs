using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Security;

namespace ATMSimulador.Domain.Validations
{
    public static class UsuarioDomain
    {
        public static Response<Usuario> CreateUser(UsuarioDto usuarioDto)
        {
            if (string.IsNullOrWhiteSpace(usuarioDto.NombreUsuario))
                return Response<Usuario>.Fail(UsuariosMensajes.MSU_002);

            if (string.IsNullOrWhiteSpace(usuarioDto.Pin) || usuarioDto.Pin.Length != 4)
                return Response<Usuario>.Fail(UsuariosMensajes.MSU_003);

            var user = new Usuario()
            {
                NombreUsuario = usuarioDto.NombreUsuario,
                Pin = XmlEncryptionService.ComputeMd5Hash(usuarioDto.Pin)
            };

            return Response<Usuario>.Success(user);
        }

        public static Response<bool> CheckLoginDto(UsuarioDto usuarioDto)
        {
            if (string.IsNullOrWhiteSpace(usuarioDto.NombreUsuario))
                return Response<bool>.Fail(UsuariosMensajes.MSU_002);

            if (string.IsNullOrWhiteSpace(usuarioDto.Pin))
                return Response<bool>.Fail(UsuariosMensajes.MSU_003);

            return Response<bool>.Success(true);
        }

        public static bool VerifyPin(string enteredPin, byte[] storedHash)
        {
            var enteredHash = XmlEncryptionService.ComputeMd5Hash(enteredPin);
            return storedHash == enteredHash;
        }
    }
}
