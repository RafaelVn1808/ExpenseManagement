using ExpenseManagement.DTOs;
using ExpenseManagement.Models;

namespace ExpenseManagement.Repositories
{
    public interface IExpenseRepository
    {
        Task<IEnumerable<Expense>> GetExpenses(string userId);
        Task<PagedResult<Expense>> GetExpensesPaged(ExpenseQueryParameters parameters, string userId);
        Task<Expense?> GetExpenseId(int expenseId, string userId);
        Task<Expense> Create(Expense expense);
        Task<Expense> Update(Expense expense);
        Task<Expense?> Delete(int expenseId, string userId);
        Task<IEnumerable<Expense>> GetExpensesForStats(string userId, DateTime from, DateTime to);
    }
}
