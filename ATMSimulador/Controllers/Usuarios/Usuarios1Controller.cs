using ATMSimulador.Attributes;
using ATMSimulador.Domain.Dtos;
using ATMSimulador.Features.Auth;
using ATMSimulador.Features.Usuarios;
using ATMSimulador.SOAP;
using ATMSimulador.SOAP.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace ATMSimulador.Controllers.Usuarios
{
    [SOAPController(SOAPVersion.v1_1)]
    public class Usuarios1Controller
        (IUsuariosService usuariosService,
        IAuthService authService,
        IWebHostEnvironment env
        ) : SOAPControllerBase(env)
    {
        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] UsuarioDto user)
        {
            var response = await usuariosService.RegistroAsync(user);
            if (response.Ok)
            {
                return Ok(response.Data);
            }

            return BadRequest(response.Message);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(SOAP1_1RequestEnvelope envelope)
        {
            var respuesta = CreateSOAPResponseEnvelope();

            if (envelope.Body?.LoginRequest == null)
            {
                return BadRequest("Request SOAP invalido.");
            }

            var loginRequest = envelope.Body.LoginRequest;

            var usuario = await usuariosService.LoginAsync(new UsuarioDto()
            {
                NombreUsuario = loginRequest.NombreUsuario!,
                Pin = loginRequest.Pin!
            });

            if (usuario.Ok)
            {
                var token = authService.GenerateToken(usuario.Data!.UsuarioId, usuario.Data.NombreUsuario);
                var tokenDescriptor = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var expires = tokenDescriptor.ValidTo;

                var tokenDto = new TokenDto
                {
                    AccessToken = token,
                    ExpiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds,
                    Exp = new DateTimeOffset(expires).ToUnixTimeSeconds()
                };

                respuesta.Body.GetLoginResponse = new()
                {
                    Token = new()
                    {
                        access_token = tokenDto.AccessToken,
                        token_type = tokenDto.TokenType,
                        expires_in = tokenDto.ExpiresIn,
                        exp = tokenDto.Exp,
                        refresh_token = tokenDto.RefreshToken
                    }
                };
                return Ok(respuesta);
            }

            return NotFound(respuesta);
        }

        [Authorize]
        [HttpGet("GetUserData")]
        public IActionResult GetUserData()
        {
            var userId = User.FindFirst("userId")?.Value;

            return Ok(new { UserId = userId });
        }
    }
}
