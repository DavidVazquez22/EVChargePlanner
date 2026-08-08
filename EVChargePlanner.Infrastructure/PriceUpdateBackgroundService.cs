using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EVChargePlanner.Domain;

namespace EVChargePlanner.Infrastructure;

public class PriceUpdateBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public PriceUpdateBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
            {
                try
            {
                await UpdatePricesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating prices: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task UpdatePricesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var priceProvider = scope.ServiceProvider.GetRequiredService<IPriceProvider>();
        var dbContext = scope.ServiceProvider.GetRequiredService<EVChargePlannerDbContext>();

        var zone = "NO1";
        var datesToTry = new[] { DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(1)) };

        foreach (var date in datesToTry)
        {
            try
            {
                var prices = await priceProvider.GetPricesAsync(date, zone);
                var deduplicatedPrices = prices.GroupBy(p => p.TimeStart).Select(g => g.First()).ToList();

                var existingTimestamps = dbContext.PriceRecords
                    .Where(p => p.PriceZone == zone)
                    .Select(p => p.TimeStart)
                    .ToHashSet();

                var newPrices = deduplicatedPrices.Where(p => !existingTimestamps.Contains(p.TimeStart)).ToList();

                if (newPrices.Count > 0)
                {
                    dbContext.PriceRecords.AddRange(newPrices);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching prices for {date}: {ex.Message}");
            }
        }
    }
}
