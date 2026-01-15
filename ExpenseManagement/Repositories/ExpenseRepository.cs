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

        public async Task<IEnumerable<Expense>> GetExpenses(string userId)
        {
            return await _context.Expenses
                .Include(c => c.Category)
                .Where(e => e.UserId == userId)
                .ToListAsync();
        }

        public async Task<Expense?> GetExpenseId(int expenseId, string userId)
        {
            return await _context.Expenses
                .Include(c => c.Category)
                .Where(e => e.ExpenseId == expenseId && e.UserId == userId)
                .FirstOrDefaultAsync();
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

        public async Task<Expense?> Delete(int expenseId, string userId)
        {
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.ExpenseId == expenseId && e.UserId == userId);

            if (expense == null)
                return null;

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return expense;
        }

    }
}
