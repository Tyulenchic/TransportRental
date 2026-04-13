using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TransportRental.Core.Models;

namespace TransportRental.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<VehicleCategory> VehicleCategories => Set<VehicleCategory>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Rental> Rentals => Set<Rental>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Role
        modelBuilder.Entity<Role>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Name).HasMaxLength(50).IsRequired();
            e.HasIndex(r => r.Name).IsUnique();
        });

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Username).HasMaxLength(100).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
            e.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            e.HasIndex(u => u.Username).IsUnique();
            e.HasOne(u => u.Role).WithMany(r => r.Users).HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
        });

        // VehicleCategory
        modelBuilder.Entity<VehicleCategory>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
            e.HasIndex(c => c.Name).IsUnique();
        });

        // Vehicle
        modelBuilder.Entity<Vehicle>(e =>
        {
            e.HasKey(v => v.Id);
            e.Property(v => v.Name).HasMaxLength(200).IsRequired();
            e.Property(v => v.LicensePlate).HasMaxLength(20);
            e.Property(v => v.HourlyRate).HasColumnType("decimal(10,2)");
            e.Property(v => v.DailyRate).HasColumnType("decimal(10,2)");
            e.HasOne(v => v.Category).WithMany(c => c.Vehicles).HasForeignKey(v => v.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        // Customer
        modelBuilder.Entity<Customer>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
            e.Property(c => c.LastName).HasMaxLength(100).IsRequired();
            e.Property(c => c.MiddleName).HasMaxLength(100);
            e.Property(c => c.Phone).HasMaxLength(20).IsRequired();
            e.Property(c => c.Email).HasMaxLength(200);
            e.Property(c => c.PassportNumber).HasMaxLength(20).IsRequired();
            e.Property(c => c.Address).HasMaxLength(500);
            e.Ignore(c => c.FullName);
        });

        // Rental
        modelBuilder.Entity<Rental>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.TotalPrice).HasColumnType("decimal(10,2)");
            e.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(r => r.Notes).HasMaxLength(1000);
            e.HasOne(r => r.Vehicle).WithMany(v => v.Rentals).HasForeignKey(r => r.VehicleId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.Customer).WithMany(c => c.Rentals).HasForeignKey(r => r.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.User).WithMany(u => u.Rentals).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        // Payment
        modelBuilder.Entity<Payment>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Amount).HasColumnType("decimal(10,2)");
            e.Property(p => p.Method).HasConversion<string>().HasMaxLength(20);
            e.HasOne(p => p.Rental).WithMany(r => r.Payments).HasForeignKey(p => p.RentalId).OnDelete(DeleteBehavior.Restrict);
        });

        // Convert all DateTime properties to UTC for PostgreSQL compatibility
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v.Value.ToUniversalTime()) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                    property.SetValueConverter(dateTimeConverter);
                else if (property.ClrType == typeof(DateTime?))
                    property.SetValueConverter(nullableDateTimeConverter);
            }
        }
    }
}
