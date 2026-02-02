using ExpenseApi.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ExpenseApi.Context
{
    public class DesignTimeApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
                basePath = Path.Combine(basePath, "ExpenseManagement");
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection")
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionString 'DefaultConnection' não encontrada. " +
                    "Defina em appsettings.json, appsettings.Development.json, " +
                    "ou nas variáveis ConnectionStrings__DefaultConnection / DEFAULT_CONNECTION.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__ApplicationDbContextMigrationsHistory"));
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
