using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;

namespace ExpenseWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
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

            // Criar claims do usuário para autenticação (inclui roles do JWT)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, model.Email)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(loginResponse.Token);
            claims.AddRange(jwt.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => new Claim(ClaimTypes.Role, c.Value)));
            claims.AddRange(jwt.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => new Claim(ClaimTypes.NameIdentifier, c.Value)));

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            await HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Expense");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
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

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var error = await _authService.ChangePasswordAsync(model.CurrentPassword, model.NewPassword);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                return View(model);
            }

            TempData["SuccessMessage"] = "Senha alterada com sucesso.";
            return RedirectToAction("Index", "Expense");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _authService.ForgotPasswordAsync(model.Email);
            TempData["SuccessMessage"] = "Se o e-mail estiver cadastrado, você receberá instruções para redefinir sua senha.";
            return RedirectToAction(nameof(Login));
        }
    }
}
