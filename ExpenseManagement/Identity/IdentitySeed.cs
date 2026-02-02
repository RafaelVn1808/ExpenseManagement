using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ExpenseApi.Identity
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 🔹 ROLES
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            // 🔹 USUÁRIO ADMIN: criado quando Admin:Email/Password ou ADMIN_EMAIL/ADMIN_PASSWORD estão definidos
            // Em Produção (ex: Render): defina ADMIN_EMAIL e ADMIN_PASSWORD nas Environment Variables
            var adminEmail = configuration["Admin:Email"] ?? configuration["ADMIN_EMAIL"];
            var adminPassword = configuration["Admin:Password"] ?? configuration["ADMIN_PASSWORD"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
