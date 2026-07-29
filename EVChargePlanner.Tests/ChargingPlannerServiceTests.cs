using EVChargePlanner.Domain;
using EVChargePlanner.Domain.Services;
using Xunit;

namespace EVChargePlanner.Tests;

public class ChargingPlannerServiceTests
{
    [Fact]
    public void FindCheapestWindow_FindsTheCorrectCheapestConsecutiveHours()
    {
        var service = new ChargingPlannerService();

        var prices = new List<PriceRecord>
        {
            CreatePrice(hour: 0, price: 1.0m),
            CreatePrice(hour: 1, price: 0.5m),
            CreatePrice(hour: 2, price: 0.5m),
            CreatePrice(hour: 3, price: 0.5m),
            CreatePrice(hour: 4, price: 2.0m),
            CreatePrice(hour: 5, price: 2.0m),
        };

        var result = service.FindCheapestWindow(prices, hoursNeeded: 3);

        Assert.NotNull(result);
        Assert.Equal(1.5m, result!.TotalPricePerKWh);
        Assert.Equal(new DateTime(2026, 1, 1, 1, 0, 0), result.StartTime);
    }

    [Fact]
    public void FindCheapestWindow_WithNotEnoughHours_ReturnsNull()
    {
        var service = new ChargingPlannerService();

        var prices = new List<PriceRecord>
        {
            CreatePrice(hour: 0, price: 1.0m),
            CreatePrice(hour: 1, price: 1.0m),
        };

        var result = service.FindCheapestWindow(prices, hoursNeeded: 5);

        Assert.Null(result);
    }

    private static PriceRecord CreatePrice(int hour, decimal price)
    {
        var baseDate = new DateTime(2026, 1, 1);
        return new PriceRecord
        {
            TimeStart = baseDate.AddHours(hour),
            TimeEnd = baseDate.AddHours(hour + 1),
            PricePerKWh = price,
            Currency = "NOK",
            PriceZone = "NO1"
        };
    }
}