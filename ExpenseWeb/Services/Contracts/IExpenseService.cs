using ExpenseWeb.Models;

namespace ExpenseWeb.Services.Contracts
{
    public interface IExpenseService
    {
        Task<PagedResult<ExpenseViewModel>> GetExpensesPaged(ExpenseQueryParameters parameters);
        Task<ExpenseViewModel> GetExpenseById(int id);
        Task<ExpenseViewModel> CreateExpense(ExpenseViewModel expense);
        Task<ExpenseViewModel> UpdateExpense(ExpenseViewModel expense);
        Task<bool> DeleteExpense(int id);
        Task<DashboardStatsViewModel?> GetDashboardStatsAsync(DateTime from, DateTime to);
    }
}
