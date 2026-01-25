using ExpenseManagement.Context;
using ExpenseManagement.DTOs;
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

        public async Task<PagedResult<Expense>> GetExpensesPaged(ExpenseQueryParameters parameters, string userId)
        {
            var query = _context.Expenses
                .Include(c => c.Category)
                .Where(e => e.UserId == userId);

            if (parameters.From.HasValue)
            {
                query = query.Where(e => e.StartDate >= parameters.From.Value);
            }

            if (parameters.To.HasValue)
            {
                query = query.Where(e => e.StartDate <= parameters.To.Value);
            }

            if (parameters.CategoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == parameters.CategoryId.Value);
            }

            if (parameters.Status.HasValue)
            {
                query = query.Where(e => e.Status == parameters.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var search = parameters.Search.Trim();
                query = query.Where(e => e.Name != null && EF.Functions.Like(e.Name, $"%{search}%"));
            }

            var sortBy = parameters.SortBy?.Trim().ToLower();
            var sortAsc = string.Equals(parameters.SortDir, "asc", StringComparison.OrdinalIgnoreCase);

            query = sortBy switch
            {
                "name" => sortAsc ? query.OrderBy(e => e.Name) : query.OrderByDescending(e => e.Name),
                "totalamount" => sortAsc ? query.OrderBy(e => e.TotalAmount) : query.OrderByDescending(e => e.TotalAmount),
                "status" => sortAsc ? query.OrderBy(e => e.Status) : query.OrderByDescending(e => e.Status),
                "category" => sortAsc ? query.OrderBy(e => e.Category!.Name) : query.OrderByDescending(e => e.Category!.Name),
                "validity" => sortAsc ? query.OrderBy(e => e.Validity) : query.OrderByDescending(e => e.Validity),
                _ => sortAsc ? query.OrderBy(e => e.StartDate) : query.OrderByDescending(e => e.StartDate)
            };

            var totalCount = await query.CountAsync();
            var skip = (parameters.Page - 1) * parameters.PageSize;
            var items = await query.Skip(skip).Take(parameters.PageSize).ToListAsync();

            return new PagedResult<Expense>
            {
                Page = parameters.Page,
                PageSize = parameters.PageSize,
                TotalCount = totalCount,
                Items = items
            };
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

        public async Task<Expense> Update(Expense expense)
        {
            if (expense == null)
            {
                throw new ArgumentNullException(nameof(expense));
            }

            var entry = _context.Entry(expense);
            if (entry.State == EntityState.Detached)
            {
                _context.Attach(expense);
                entry = _context.Entry(expense);
            }
            entry.State = EntityState.Modified;
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
