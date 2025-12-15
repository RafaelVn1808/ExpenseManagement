using ExpenseWeb.Models;

namespace ExpenseWeb.Services.Contracts
{
    public interface IExpensiveService
    {
        Task<IEnumerable<ExpenseViewModel>> GetAllExpenses();
        Task<ExpenseViewModel> GetExpenseById(int id);
        Task<ExpenseViewModel> CreateExpense(ExpenseViewModel expense);
        Task<ExpenseViewModel> UpdateExpense(int id, ExpenseViewModel expense);
        Task<bool> DeleteExpense(int id);
    }
}
