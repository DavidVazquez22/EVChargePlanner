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
    public DbSet<User> Users { get; set; }
    public DbSet<CarModel> CarModels { get; set; }

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

        modelBuilder.Entity<CarModel>().HasData(
            new CarModel { Id = 1, Brand = "Tesla", Model = "Model 3", BatteryCapacityKWh = 57.5m, MaxChargingPowerKW = 11m },
            new CarModel { Id = 2, Brand = "Tesla", Model = "Model Y", BatteryCapacityKWh = 75m, MaxChargingPowerKW = 11m },
            new CarModel { Id = 3, Brand = "Volkswagen", Model = "ID.3", BatteryCapacityKWh = 58m, MaxChargingPowerKW = 11m },
            new CarModel { Id = 4, Brand = "Volkswagen", Model = "ID.4", BatteryCapacityKWh = 77m, MaxChargingPowerKW = 11m },
            new CarModel { Id = 5, Brand = "BYD", Model = "Dolphin", BatteryCapacityKWh = 60.4m, MaxChargingPowerKW = 11m },
            new CarModel { Id = 6, Brand = "BYD", Model = "Atto 3", BatteryCapacityKWh = 60.5m, MaxChargingPowerKW = 11m },
            new CarModel { Id = 7, Brand = "Hyundai", Model = "Kona Electric", BatteryCapacityKWh = 65.4m, MaxChargingPowerKW = 11m },
            new CarModel { Id = 8, Brand = "Hyundai", Model = "Ioniq 5", BatteryCapacityKWh = 77.4m, MaxChargingPowerKW = 11m },
            new CarModel { Id = 9, Brand = "Toyota", Model = "RAV4 Plug-in Hybrid", BatteryCapacityKWh = 18.1m, MaxChargingPowerKW = 6.6m },
            new CarModel { Id = 10, Brand = "Volvo", Model = "XC60 Recharge", BatteryCapacityKWh = 18.8m, MaxChargingPowerKW = 6.4m }
        );
    }

}