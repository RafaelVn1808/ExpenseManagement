using ExpenseApi.Identity;
using ExpenseApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpenseApi.Controllers
{
    [ApiController]
    [Authorize(Roles = Roles.Admin)]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserRolesDto>>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = new List<UserRolesDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserRolesDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList()
                });
            }

            return Ok(result.OrderBy(u => u.Email));
        }

        [HttpPost("users/{userId}/roles")]
        public async Task<IActionResult> UpdateUserRoles(
            string userId,
            [FromBody] UpdateUserRolesDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Payload inválido." });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado." });
            }

            var allowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                Roles.Admin,
                Roles.User
            };

            var requestedRoles = request.Roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (requestedRoles.Any(role => !allowedRoles.Contains(role)))
            {
                return BadRequest(new { message = "Role inválida informada." });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isSelf = string.Equals(currentUserId, userId, StringComparison.Ordinal);
            if (isSelf && !requestedRoles.Contains(Roles.Admin, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Você não pode remover seu próprio acesso de admin." });
            }

            foreach (var role in requestedRoles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    return BadRequest(new { message = $"Role '{role}' não existe." });
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToAdd = requestedRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToList();
            var rolesToRemove = currentRoles.Except(requestedRoles, StringComparer.OrdinalIgnoreCase).ToList();

            if (rolesToAdd.Count > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    _logger.LogWarning("Falha ao adicionar roles para {UserId}: {Errors}",
                        userId,
                        string.Join(", ", addResult.Errors.Select(e => e.Description)));
                    return BadRequest(new { message = "Falha ao adicionar roles." });
                }
            }

            if (rolesToRemove.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    _logger.LogWarning("Falha ao remover roles para {UserId}: {Errors}",
                        userId,
                        string.Join(", ", removeResult.Errors.Select(e => e.Description)));
                    return BadRequest(new { message = "Falha ao remover roles." });
                }
            }

            var updatedRoles = await _userManager.GetRolesAsync(user);
            return Ok(new UserRolesDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = updatedRoles.ToList()
            });
        }
    }
}
