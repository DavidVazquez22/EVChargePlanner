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
}