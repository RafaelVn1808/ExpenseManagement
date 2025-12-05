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

        public async Task<IEnumerable<Expense>> GetExpenses()
        {
            return await _context.Expenses.ToListAsync();
        }

        public async Task<Expense> GetExpenseId(int expenseId)
        {
            return await _context.Expenses.Where(e => e.ExpenseId == expenseId).FirstOrDefaultAsync();
        }

        public async Task<Expense> Create(Expense expense)
        {
            if (expense == null)
            {
                throw new ArgumentNullException(nameof(expense));
            }

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task<Expense >Update(Expense expense)
        {
            if (expense == null)
            {
                throw new ArgumentNullException(nameof(expense));
            }

            _context.Entry(expense).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return expense;
        }

        public async Task<Expense> Delete(int expenseId)
        {
            var expense = _context.Expenses.Find(expenseId);

            if (expense != null)
            {
                throw new ArgumentNullException(nameof(expense));
            }
            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return expense;

        }
    }
}
