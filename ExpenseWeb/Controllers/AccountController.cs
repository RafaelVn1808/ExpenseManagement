using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var token = await _authService.LoginAsync(model.Email, model.Password);

            if (string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError("", "Login inválido");
                return View(model);
            }

            HttpContext.Session.SetString("JWToken", token);

            return RedirectToAction("Index", "Expense");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
