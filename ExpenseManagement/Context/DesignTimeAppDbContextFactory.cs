using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ExpenseManagement.Context
{
    public class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Defina ConnectionStrings__DefaultConnection ou DEFAULT_CONNECTION (variável de ambiente ou User Secrets).");
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
