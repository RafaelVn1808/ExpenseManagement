using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseWeb.Controllers
{
    [AllowAnonymous]
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

            var loginResponse = await _authService.LoginAsync(model.Email, model.Password);

            if (loginResponse == null || string.IsNullOrEmpty(loginResponse.Token))
            {
                ModelState.AddModelError("", "Email ou senha inválidos");
                return View(model);
            }

            HttpContext.Session.SetString("JWToken", loginResponse.Token);
            HttpContext.Session.SetString("RefreshToken", loginResponse.RefreshToken);
            HttpContext.Session.SetString("JwtExpiresAt", loginResponse.ExpiresAt.ToUniversalTime().ToString("O"));

            // Criar claims do usuário para autenticação
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, model.Email)
            };

            var claimsIdentity = new System.Security.Claims.ClaimsIdentity(claims, "CookieAuth");
            await HttpContext.SignInAsync("Cookies", new System.Security.Claims.ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Expense");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var success = await _authService.RegisterAsync(model.Email, model.Password);

            if (!success)
            {
                ModelState.AddModelError("", "Erro ao criar conta. Verifique se o email já não está cadastrado.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Conta criada com sucesso! Faça login para continuar.";
            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction(nameof(Login));
        }
    }
}
