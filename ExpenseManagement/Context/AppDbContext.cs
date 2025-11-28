using ExpenseManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManagement.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {    
        }

        public DbSet<Category>? Categories { get; set; }
        public DbSet<Expense>? Expenses { get; set; }
    }
}
