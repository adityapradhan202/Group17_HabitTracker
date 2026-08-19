using HabitTrackerAspNetMVCWebApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HabitTrackerAspNetMVCWebApp.Data
{
    public static class DbInitializer
    {
        public const string DefaultAdminEmail = "zasadna@gmail.com";
        public const string DefaultAdminPassword = "Qw123456$";

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await SeedRolesAsync(roleManager);
            await SeedDefaultAdminAsync(userManager);
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

        public static async Task SeedDefaultAdminAsync(UserManager<ApplicationUser> userManager)
        {
            var admin = await userManager.Users
                .FirstOrDefaultAsync(u => u.Email == DefaultAdminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = DefaultAdminEmail,
                    Email = DefaultAdminEmail,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createResult = await userManager.CreateAsync(admin, DefaultAdminPassword);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create default admin: {errors}");
                }
            }

            if (!admin.IsActive)
            {
                admin.IsActive = true;
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