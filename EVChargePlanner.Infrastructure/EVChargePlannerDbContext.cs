using EVChargePlanner.Domain;
using Microsoft.EntityFrameworkCore;

namespace EVChargePlanner.Infrastructure;

public class EVChargePlannerDbContext : DbContext
{
    public EVChargePlannerDbContext(DbContextOptions<EVChargePlannerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Car> Cars { get; set; }
    public DbSet<Charger> Chargers { get; set; }
    public DbSet<ChargingSession> ChargingSessions { get; set; }
    public DbSet<PriceRecord> PriceRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PriceRecord>()
            .HasIndex(p => new { p.TimeStart, p.PriceZone })
            .IsUnique();

        var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? (v.Value.Kind == DateTimeKind.Utc ? v.Value : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc)) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

        modelBuilder.Entity<Car>()
            .Property(c => c.DepartureTime)
            .HasConversion(nullableDateTimeConverter);

        modelBuilder.Entity<PriceRecord>()
            .Property(p => p.TimeStart)
            .HasConversion(dateTimeConverter);

        modelBuilder.Entity<PriceRecord>()
            .Property(p => p.TimeEnd)
            .HasConversion(dateTimeConverter);

        modelBuilder.Entity<ChargingSession>()
            .Property(s => s.StartTime)
            .HasConversion(dateTimeConverter);

        modelBuilder.Entity<ChargingSession>()
            .Property(s => s.EndTime)
            .HasConversion(dateTimeConverter);
    }

}