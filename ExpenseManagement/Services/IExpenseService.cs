using ExpenseManagement.DTOs;

namespace ExpenseManagement.Services
{
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseDTO>> GetAllExpensesAsync();
        Task<ExpenseDTO> GetExpensesByIdAsync(int id);
        Task CreateExpensesAsync(ExpenseDTO productDto);
        Task UpdateExpenseAsync(ExpenseDTO productDto);
        Task DeleteExpenseAsync(int id);

    }
}
