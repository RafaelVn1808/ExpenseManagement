using ExpenseApi.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseApi.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RefreshToken>()
                .Property(rt => rt.Token)
                .HasMaxLength(450);

            builder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();
        }

        public override int SaveChanges()
        {
            ConvertDatesToUtc();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ConvertDatesToUtc();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ConvertDatesToUtc()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(DateTime))
                    {
                        var value = (DateTime?)property.CurrentValue;
                        if (value.HasValue && value.Value.Kind == DateTimeKind.Unspecified)
                        {
                            property.CurrentValue = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                        }
                    }
                    else if (property.Metadata.ClrType == typeof(DateTime?))
                    {
                        var value = (DateTime?)property.CurrentValue;
                        if (value.HasValue && value.Value.Kind == DateTimeKind.Unspecified)
                        {
                            property.CurrentValue = DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
                        }
                    }
                }
            }
        }
    }
}
