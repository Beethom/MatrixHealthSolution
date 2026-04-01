using MatrixHealthSolution.Data;
using MatrixHealthSolution.Data.Seed;
using MatrixHealthSolution.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Stripe;

var builder = WebApplication.CreateBuilder(args);

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(
//        builder.Configuration.GetConnectionString("DefaultConnection"),
//        sql =>
//        {
//            sql.EnableRetryOnFailure(
//                maxRetryCount: 5,
//                maxRetryDelay: TimeSpan.FromSeconds(10),
//                errorNumbersToAdd: null
//            );
//
//            sql.CommandTimeout(60); // optional (seconds)
//        }
//    )
//);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=matrixhealthsolution.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<MatrixHealthSolution.Services.EmailService>();

builder.Services.AddScoped<MatrixHealthSolution.Services.AppointmentSlotService>();

// ✅ Session for Cart
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // ✅ session expires after 2 hours idle
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    try
    {
        await DbSeeder.SeedSampleDataAsync(context, userManager, roleManager);
        Console.WriteLine("Database seeding completed!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding database: {ex.Message}");
    }
}

var logger = app.Services.GetRequiredService<ILogger<Program>>();
if (string.IsNullOrEmpty(app.Configuration["Stripe:SecretKey"]))
    logger.LogWarning("Stripe:SecretKey is not configured. Payments will not work.");
if (string.IsNullOrEmpty(app.Configuration["Stripe:WebhookSecret"]))
    logger.LogWarning("Stripe:WebhookSecret is not configured. Webhook verification will fail.");

app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // ✅ must be before endpoints + before you read session

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
