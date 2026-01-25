using ExpenseWeb.Models;

namespace ExpenseWeb.Services.Contracts
{
    public interface IAdminService
    {
        Task<IReadOnlyList<AdminUserViewModel>> GetUsersAsync();
        Task<bool> UpdateUserRolesAsync(string userId, IEnumerable<string> roles);
    }
}
