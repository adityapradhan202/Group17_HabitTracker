using HabitTracker.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HabitTracker.Api.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            await SeedRolesAsync(roleManager);
            await SeedDefaultAdminAsync(userManager, configuration);
        }

        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = ["Admin", "User"];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedDefaultAdminAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            string adminEmail = configuration["AdminSeed:Email"] ?? "adityapradhan5060@gmail.com";
            string adminPassword = configuration["AdminSeed:Password"] ?? "abc123ABC!";

            var admin = await userManager.Users
                .FirstOrDefaultAsync(u => u.Email == adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createResult = await userManager.CreateAsync(admin, adminPassword);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create default admin ({adminEmail}): {errors}");
                }
            }
            else
            {
                // Reset password if user already exists to ensure configured secret password is valid
                var hasPassword = await userManager.HasPasswordAsync(admin);
                if (hasPassword)
                {
                    await userManager.RemovePasswordAsync(admin);
                }
                var addPasswordResult = await userManager.AddPasswordAsync(admin, adminPassword);
                if (!addPasswordResult.Succeeded)
                {
                    var errors = string.Join("; ", addPasswordResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to update default admin password: {errors}");
                }
                admin.IsActive = true;
                admin.EmailConfirmed = true;
                await userManager.UpdateAsync(admin);
            }

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            if (!await userManager.IsInRoleAsync(admin, "User"))
            {
                await userManager.AddToRoleAsync(admin, "User");
            }
        }
    }
}
