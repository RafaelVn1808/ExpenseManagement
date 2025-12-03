using ExpenseManagement.Context;
using ExpenseManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManagement.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly AppDbContext _context;

        public ExpenseRepository(AppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Expense> GetExpenses()
        {
            return _context.Expenses.ToList();
        }

        public Expense GetExpenseId(int expenseId)
        {
           return _context.Expenses.FirstOrDefault(e => e.ExpenseId == expenseId);
        }

        public Expense Create(Expense expense)
        {
            if (expense == null)
            {
                throw new ArgumentNullException(nameof(expense));
            }

            _context.Expenses.Add(expense);
            _context.SaveChanges();
            return expense;
        }

        public Expense Update(Expense expense)
        {
            if (expense == null)
            {
                throw new ArgumentNullException(nameof(expense));
            }

            _context.Entry(expense).State = EntityState.Modified;
            _context.SaveChanges();
            return expense;
        }

        public Expense Delete(int expenseId)
        {
            var expense = _context.Expenses.Find(expenseId);

            if (expense != null)
            {
                throw new ArgumentNullException(nameof(expense));
            }
            _context.Expenses.Remove(expense);
            _context.SaveChanges();
            return expense;

        }
    }
}
