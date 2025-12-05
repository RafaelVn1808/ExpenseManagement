using ExpenseManagement.Models;

namespace ExpenseManagement.Repositories
{
    public interface IExpenseRepository
    {

        Task<IEnumerable<Expense>> GetExpenses();
        Task<Expense> GetExpenseId(int expenseId);
        Task<Expense> Create(Expense expense);
        Task<Expense> Update(Expense expense);
        Task<Expense> Delete(int expenseId);

    }
}
