using MatrixHealthSolution.Models;
using MatrixHealthSolution.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
        using MatrixHealthSolution.Models.Enums;

namespace MatrixHealthSolution.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedSampleDataAsync(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        // EnsureCreated creates the schema from the current model (SQLite-compatible).
        // MigrateAsync cannot be used here because InitialCreate was written for SQL Server.
        await context.Database.EnsureCreatedAsync();

        // Skip seeding entirely if already done
        bool rolesExist = await roleManager.RoleExistsAsync("Admin");
        bool dataExists = await context.Services.AnyAsync();
        if (rolesExist && dataExists)
            return;

        // --- Roles ---
        string[] roles = new[] { "Admin", "Employee", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // --- Admin user (if not exists) ---
        string adminEmail = "admin@matrixhealth.com";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin123!"); // temporary password
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // --- Employees ---
        if (!context.Employees.Any())
        {
            context.Employees.AddRange(
                new Employee { FullName = "Alice Johnson", Email = "alice@matrixhealth.com", Position = "Nurse" },
                new Employee { FullName = "Bob Smith", Email = "bob@matrixhealth.com", Position = "Therapist" },
                new Employee { FullName = "Carol Lee", Email = "carol@matrixhealth.com", Position = "Doctor" }
            );
        }

        // --- Services ---
        if (!context.Services.Any())
        {
            context.Services.AddRange(
                new Service { Name = "IV Vitamin Therapy", Description = "Boost your vitamins.", Price = 150 },
                new Service { Name = "Physical Exam", Description = "Comprehensive physical examination.", Price = 100 },
                new Service { Name = "Wellness Consultation", Description = "Health consultation with doctor.", Price = 75 }
            );
        }

        // --- Products ---
        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Product { Name = "Vitamin C 500mg", Description = "Immune support.", Price = 20, Stock = 50 },
                new Product { Name = "Omega 3 Capsules", Description = "Heart health.", Price = 25, Stock = 30 },
                new Product { Name = "Protein Powder", Description = "Muscle support.", Price = 40, Stock = 20 }
            );
        }

        // --- Memberships ---

if (!context.Memberships.Any())
{
    context.Memberships.AddRange(
        new Membership
        {
            Name = "Monthly Wellness",
            Description = "Billed monthly. Cancel anytime.",
            Price = 199,
            Period = MembershipPeriod.Monthly,
            IsActive = true
        },
        new Membership
        {
            Name = "3-Month Wellness",
            Description = "Best value for short-term commitment.",
            Price = 549,
            Period = MembershipPeriod.ThreeMonths,
            IsActive = true
        },
        new Membership
        {
            Name = "Annual Wellness",
            Description = "Our best deal for a full year.",
            Price = 1999,
            Period = MembershipPeriod.Yearly,
            IsActive = true
        }
    );
}


        // --- Gift Cards ---
        if (!context.GiftCards.Any())
        {
            context.GiftCards.AddRange(
                new GiftCard { Code = "GIFT50", Value = 50, ExpiryDate = DateTime.UtcNow.AddMonths(12) },
                new GiftCard { Code = "GIFT100", Value = 100, ExpiryDate = DateTime.UtcNow.AddMonths(12) }
            );
        }

        await context.SaveChangesAsync();
    }
}
