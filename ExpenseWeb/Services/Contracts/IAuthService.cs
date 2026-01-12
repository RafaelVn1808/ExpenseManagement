using ExpenseWeb.Models;

namespace ExpenseWeb.Services.Contracts
{
    public interface IAuthService
    {
        Task<string?> Login(LoginViewModel model);
    }
}
