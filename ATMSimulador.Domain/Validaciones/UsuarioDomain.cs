using ATMSimulador.Domain.Dtos;
using ATMSimulador.Domain.Entities;
using ATMSimulador.Domain.Mensajes;
using ATMSimulador.Domain.Security;

namespace ATMSimulador.Domain.Validations
{
    public class UsuarioDomain
    {
        private readonly XmlEncryptionService _xmlEncryptionService;

        public UsuarioDomain(XmlEncryptionService xmlEncryptionService)
        {
            _xmlEncryptionService = xmlEncryptionService;
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
                Pin = _xmlEncryptionService.ComputeMd5Hash(usuarioDto.Pin)
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

        public bool VerifyPin(string enteredPin, string storedHash)
        {
            var enteredHash = _xmlEncryptionService.ComputeMd5Hash(enteredPin);
            return storedHash == enteredHash;
        }
    }
}
