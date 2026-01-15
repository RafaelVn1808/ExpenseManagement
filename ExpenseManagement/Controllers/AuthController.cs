using ExpenseApi.Identity;
using ExpenseApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace ExpenseApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _config = config;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
            {
                return BadRequest(new { message = "Dados de registro inválidos." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verificar se o email já existe
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Tentativa de registro com email já existente: {Email}", registerDto.Email);
                return BadRequest(new { message = "Este email já está cadastrado." });
            }

            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                EmailConfirmed = true // Em produção, considerar confirmação por email
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                _logger.LogWarning("Falha ao criar usuário: {Email}, Erros: {Errors}", registerDto.Email, string.Join(", ", errors));
                return BadRequest(new { message = "Falha ao criar usuário.", errors });
            }

            // Adicionar ao role padrão "User"
            await _userManager.AddToRoleAsync(user, "User");

            _logger.LogInformation("Novo usuário registrado: {Email}", registerDto.Email);

            return Ok(new { message = "Usuário registrado com sucesso." });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            if (login == null || string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password))
            {
                return BadRequest(new { message = "Email e senha são obrigatórios." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(login.Email);

            if (user == null)
            {
                _logger.LogWarning("Tentativa de login com email inexistente: {Email}", login.Email);
                // Retornar mesmo erro para não revelar se o email existe
                return Unauthorized(new { message = "Email ou senha inválidos." });
            }

            // Verificar se a conta está bloqueada
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Tentativa de login em conta bloqueada: {Email}", login.Email);
                return Unauthorized(new { message = "Conta temporariamente bloqueada. Tente novamente mais tarde." });
            }

            if (!await _userManager.CheckPasswordAsync(user, login.Password))
            {
                // Incrementar contador de tentativas falhadas
                await _userManager.AccessFailedAsync(user);
                _logger.LogWarning("Tentativa de login falhada: {Email}", login.Email);
                return Unauthorized(new { message = "Email ou senha inválidos." });
            }

            // Resetar contador de tentativas falhadas em caso de sucesso
            await _userManager.ResetAccessFailedCountAsync(user);

            var token = GenerateJwt(user);

            _logger.LogInformation("Login bem-sucedido: {Email}", login.Email);

            return Ok(new { token });
        }

        private string GenerateJwt(ApplicationUser user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(
                    int.Parse(_config["Jwt:ExpireHours"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
