//using ATMSimulador.Domain.Dtos;
//using ATMSimulador.Features.Auth;
//using ATMSimulador.Features.Usuarios;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.IdentityModel.Tokens.Jwt;

//namespace ATMSimulador.Controllers.Usuarios
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class UsuariosController
//        (IUsuariosService usuariosService,
//        IAuthService authService,
//        IWebHostEnvironment env
//        ) : ControllerBase
//    {
//        [HttpPost("registro")]
//        public async Task<IActionResult> Registro([FromBody] UsuarioDto user)
//        {
//            var response = await usuariosService.RegistroAsync(user);
//            if (response.Ok)
//            {
//                return Ok(response.Data);
//            }

//            return BadRequest(response.Message);
//        }

//        [HttpPost("login")]
//        public async Task<IActionResult> LoginAsync(UsuarioDto dto)
//        {

//            var usuario = await usuariosService.LoginAsync(dto);

//            if (usuario.Ok)
//            {
//                var token = authService.GenerateToken(usuario.Data!.UsuarioId, usuario.Data.NombreUsuario);
//                var tokenDescriptor = new JwtSecurityTokenHandler().ReadJwtToken(token);
//                var expires = tokenDescriptor.ValidTo;

//                var tokenDto = new TokenDto
//                {
//                    AccessToken = token,
//                    ExpiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds,
//                    Exp = new DateTimeOffset(expires).ToUnixTimeSeconds()
//                };

//                var respuesta = new LoginRespuestaDto()
//                {
//                    access_token = tokenDto.AccessToken,
//                    token_type = tokenDto.TokenType,
//                    expires_in = tokenDto.ExpiresIn,
//                    exp = tokenDto.Exp,
//                    refresh_token = tokenDto.RefreshToken
//                };
//                return Ok(respuesta);
//            }

//            return NotFound();
//        }

//        [Authorize]
//        [HttpGet("GetUserData")]
//        public IActionResult GetUserData()
//        {
//            var userId = User.FindFirst("userId")?.Value;

//            return Ok(new { UserId = userId });
//        }
//    }
//}
