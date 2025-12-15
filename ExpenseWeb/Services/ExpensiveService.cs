using ExpenseWeb.Models;
using ExpenseWeb.Services.Contracts;

namespace ExpenseWeb.Services
{
    public class ExpensiveService : IExpensiveService
    {
        public Task<IEnumerable<ExpenseViewModel>> GetAllExpenses()
        {
            throw new NotImplementedException();
        }

        public Task<ExpenseViewModel> GetExpenseById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ExpenseViewModel> CreateExpense(ExpenseViewModel expense)
        {
            throw new NotImplementedException();
        }

        public Task<ExpenseViewModel> UpdateExpense(int id, ExpenseViewModel expense)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteExpense(int id)
        {
            throw new NotImplementedException();
        }

    }
}
