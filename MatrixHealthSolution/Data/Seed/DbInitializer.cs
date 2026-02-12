using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MatrixHealthSolution.Models.Identity;
using MatrixHealthSolution.Models;

namespace MatrixHealthSolution.Data.Seed;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        // 🔑 Resolve services from DI
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure DB exists / migrated
        await context.Database.MigrateAsync();

        // ==========================
        // Seed Roles
        // ==========================
        string[] roles = { "Admin", "Employee", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // ==========================
        // Seed Admin User
        // ==========================
        string adminEmail = "admin@matrixhealth.com";
        string adminPassword = "Admin123!"; // 🔐 change later

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ",
                    result.Errors.Select(e => e.Description)));
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // ==========================
        // Seed Services
        // ==========================
        if (!context.Services.Any())
        {
            context.Services.AddRange(
                new Service
                {
                    Name = "Deep Tissue Massage",
                    Description = "Relieve muscle tension and stress.",
                    Price = 120,
                    IsActive = true
                },
                new Service
                {
                    Name = "Facial Therapy",
                    Description = "Rejuvenating facial treatment.",
                    Price = 90,
                    IsActive = true
                },
                new Service
                {
                    Name = "Aromatherapy",
                    Description = "Relaxing essential oil therapy.",
                    Price = 75,
                    IsActive = true
                }
            );

            await context.SaveChangesAsync();
        }
    }
}
