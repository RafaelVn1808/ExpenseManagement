namespace ExpenseWeb.Services.Contracts
{
    public interface IAuthService
    {
        Task<string?> LoginAsync(string email, string password);
    }
}
