using ExpenseApi.Models;
using ExpenseManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseApi.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Realiza login e retorna o JWT
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var token = await _authService.LoginAsync(dto.Email, dto.Password);
                return Ok(new { token });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Credenciais inválidas" });
            }
        }
    }
}
