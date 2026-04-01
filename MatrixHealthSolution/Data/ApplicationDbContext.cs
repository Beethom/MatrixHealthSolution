using MatrixHealthSolution.Models;
using MatrixHealthSolution.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MatrixHealthSolution.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<GiftCard> GiftCards => Set<GiftCard>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ScheduleOverride> ScheduleOverrides => Set<ScheduleOverride>();

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Appointment -> Service relationship
        builder.Entity<Appointment>()
            .HasOne(a => a.Service)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent double-booking the same service at the same time
        builder.Entity<Appointment>()
            .HasIndex(a => new { a.ServiceId, a.ScheduledAt })
            .IsUnique();

        // ============================================================
        // SQLite support for DateOnly/TimeOnly (ScheduleOverride)
        // ============================================================

        var dateOnlyConverter = new ValueConverter<DateOnly, string>(
            d => d.ToString("yyyy-MM-dd"),
            s => DateOnly.Parse(s));

        builder.Entity<ScheduleOverride>()
            .Property(x => x.Date)
            .HasConversion(dateOnlyConverter);

        builder.Entity<ScheduleOverride>()
            .Property(x => x.OpenTime)
            .HasConversion(
                v => v.HasValue ? v.Value.ToString("HH:mm:ss") : null,
                v => string.IsNullOrWhiteSpace(v) ? null : TimeOnly.Parse(v));

        builder.Entity<ScheduleOverride>()
            .Property(x => x.CloseTime)
            .HasConversion(
                v => v.HasValue ? v.Value.ToString("HH:mm:ss") : null,
                v => string.IsNullOrWhiteSpace(v) ? null : TimeOnly.Parse(v));

        // Orders
    builder.Entity<Order>()
        .HasMany(o => o.Items)
        .WithOne(i => i.Order)
        .HasForeignKey(i => i.OrderId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<Order>()
        .HasOne(o => o.Payment)
        .WithOne(p => p.Order)
        .HasForeignKey<Payment>(p => p.OrderId)
        .OnDelete(DeleteBehavior.Cascade);

    builder.Entity<OrderItem>()
        .HasOne(i => i.Product)
        .WithMany()
        .HasForeignKey(i => i.ProductId)
        .OnDelete(DeleteBehavior.Restrict);
    }
}
