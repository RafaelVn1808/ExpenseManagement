using ExpenseManagement.Models;

namespace ExpenseManagement.Repositories
{
    public interface IExpenseRepository
    {

        IEnumerable<Expense> GetExpenses();
        Expense GetExpenseId(int expenseId);
        Expense Create(Expense expense);
        Expense Update(Expense expense);
        Expense Delete(int expenseId);

    }
}
