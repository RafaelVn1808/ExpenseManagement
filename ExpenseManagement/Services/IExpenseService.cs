using ExpenseManagement.DTOs;

namespace ExpenseManagement.Services
{
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseDTO>> GetAllExpensesAsync(string userId);
        Task<PagedResult<ExpenseDTO>> GetExpensesPagedAsync(ExpenseQueryParameters parameters, string userId);
        Task<ExpenseDTO?> GetExpensesByIdAsync(int id, string userId);
        Task CreateExpensesAsync(ExpenseDTO expenseDTO, string userId);
        Task UpdateExpenseAsync(ExpenseDTO expenseDTO, string userId);
        Task<ExpenseDTO?> DeleteExpenseAsync(int id, string userId);
    }
}
