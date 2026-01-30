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
    }
}
