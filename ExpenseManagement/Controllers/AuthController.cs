using ExpenseApi.Context;
using ExpenseApi.Identity;
using ExpenseApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ExpenseApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IConfiguration config,
            ILogger<AuthController> logger)
        {
            _userManager = userManager;
            _context = context;
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

            var authResponse = await GenerateTokensAsync(user);

            _logger.LogInformation("Login bem-sucedido: {Email}", login.Email);

            return Ok(authResponse);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

            if (existingToken == null || !existingToken.IsActive || existingToken.User == null)
            {
                return Unauthorized(new { message = "Refresh token inválido ou expirado." });
            }

            existingToken.RevokedAt = DateTime.UtcNow;

            var authResponse = await GenerateTokensAsync(existingToken.User);

            return Ok(authResponse);
        }

        private async Task<AuthResponseDto> GenerateTokensAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!)
            };

            // 🔐 ADICIONAR ROLES AO TOKEN
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role =>
                new Claim(ClaimTypes.Role, role)
            ));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(int.Parse(_config["Jwt:ExpireHours"] ?? "2")),
                signingCredentials: creds);

            var refreshToken = GenerateRefreshToken();
            var refreshExpiresAt = DateTime.UtcNow.AddDays(
                int.Parse(_config["Jwt:RefreshTokenDays"] ?? "7"));

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = refreshExpiresAt
            });

            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                ExpiresAt = token.ValidTo
            };
        }

        private static string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>Troca de senha do usuário autenticado.</summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest(new { message = "Senha atual e nova senha são obrigatórias." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Usuário não identificado." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { message = "Não foi possível alterar a senha.", errors });
            }

            _logger.LogInformation("Senha alterada com sucesso para o usuário: {Email}", user.Email);
            return Ok(new { message = "Senha alterada com sucesso." });
        }

        /// <summary>Esqueci minha senha - envia instruções por e-mail (stub: sempre retorna sucesso por segurança).</summary>
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest(new { message = "Informe o e-mail." });
            }

            var user = await _userManager.FindByEmailAsync(dto.Email.Trim());
            if (user != null)
            {
                // Em produção: gerar token de reset, salvar em tabela com expiração, enviar e-mail com link.
                // Por ora apenas log para não revelar se o e-mail existe.
                _logger.LogInformation("Solicitação de recuperação de senha para: {Email}", dto.Email);
            }

            // Sempre retorna sucesso para não revelar se o e-mail existe no sistema.
            return Ok(new { message = "Se o e-mail estiver cadastrado, você receberá instruções para redefinir sua senha." });
        }
    }
}
