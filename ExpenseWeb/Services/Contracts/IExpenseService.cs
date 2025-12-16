using ExpenseWeb.Models;

namespace ExpenseWeb.Services.Contracts
{
    public interface IExpenseService
    {
        Task<IEnumerable<ExpenseViewModel>> GetAllExpenses();
        Task<ExpenseViewModel> GetExpenseById(int id);
        Task<ExpenseViewModel> CreateExpense(ExpenseViewModel expense);
        Task<ExpenseViewModel> UpdateExpense(ExpenseViewModel expense);
        Task<bool> DeleteExpense(int id);
    }
}
