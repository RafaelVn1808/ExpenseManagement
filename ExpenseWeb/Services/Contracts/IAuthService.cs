using ExpenseWeb.Models;

namespace ExpenseWeb.Services.Contracts
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(string email, string password);
    }
}
