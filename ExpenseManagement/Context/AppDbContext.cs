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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Expense>()
                .Property(e => e.Status)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<Expense>()
                .HasIndex(e => e.UserId);

            modelBuilder.Entity<Expense>()
                .HasIndex(e => new { e.UserId, e.StartDate });

            modelBuilder.Entity<Expense>()
                .HasIndex(e => e.CategoryId);

            modelBuilder.Entity<Expense>()
                .HasIndex(e => e.Status);
        }
    }
}
