using ExpenseManagement.DTOs;

namespace ExpenseManagement.Services
{
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseDTO>> GetAllExpensesAsync();
        Task<ExpenseDTO> GetExpensesByIdAsync(int id);
        Task CreateExpensesAsync(ExpenseDTO expenseDTO);
        Task UpdateExpenseAsync(ExpenseDTO expenseDTO);
        Task<ExpenseDTO?> DeleteExpenseAsync(int id);

    }
}
