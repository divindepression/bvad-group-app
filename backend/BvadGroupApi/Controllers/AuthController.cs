using BvadGroupApi.Dtos;
using BvadGroupApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BvadGroupApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("🔐 Authentification")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Connexion utilisateur (retourne un token JWT + infos user + filiales)
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Identifiants requis" });
            }

            var result = await _authService.LoginAsync(request);

            if (result == null)
            {
                return Unauthorized(new { message = "Identifiants invalides" });
            }

            return Ok(result);
        }
    }
}