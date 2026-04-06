using System.Security.Claims;
using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseWeb.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _adminService.GetUsersAsync();
                ViewBag.CurrentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return View(users);
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return View(new List<AdminUserViewModel>());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Erro ao carregar usuários: " + ex.Message;
                return View(new List<AdminUserViewModel>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(UpdateUserRolesViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.UserId))
            {
                TempData["ErrorMessage"] = "Usuário inválido.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _adminService.UpdateUserRolesAsync(
                model.UserId,
                model.Roles ?? new List<string>());

            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Permissões atualizadas com sucesso." : "Falha ao atualizar permissões.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                TempData["ErrorMessage"] = "Usuário inválido.";
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.Equals(currentUserId, userId, StringComparison.Ordinal))
            {
                TempData["ErrorMessage"] = "Você não pode excluir sua própria conta.";
                return RedirectToAction(nameof(Index));
            }

            var success = await _adminService.DeleteUserAsync(userId);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] =
                success ? "Usuário excluído com sucesso." : "Falha ao excluir usuário.";

            return RedirectToAction(nameof(Index));
        }
    }
}
