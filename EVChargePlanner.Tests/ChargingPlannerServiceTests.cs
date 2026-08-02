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

    [Fact]
    public void PlanForMultipleCars_PrioritizesUrgentCarForCheapestSlot()
    {
        var service = new ChargingPlannerService();
        var baseDate = new DateTime(2026, 1, 1);

        var prices = new List<PriceRecord>
        {
            CreatePrice(baseDate, hour: 0, price: 0.5m),
            CreatePrice(baseDate, hour: 1, price: 0.5m),
            CreatePrice(baseDate, hour: 2, price: 3.0m),
            CreatePrice(baseDate, hour: 3, price: 3.0m),
            CreatePrice(baseDate, hour: 4, price: 3.0m),
            CreatePrice(baseDate, hour: 5, price: 3.0m),
        };

        var urgentCar = new Car
        {
            Id = 1,
            Name = "Urgent",
            BatteryCapacityKWh = 40,
            MaxChargingPowerKW = 20,
        };

        var flexibleCar = new Car
        {
            Id = 2,
            Name = "Flexible",
            BatteryCapacityKWh = 40,
            MaxChargingPowerKW = 20,
        };

        var chargeInfos = new List<CarChargeInfo>
        {
            new() { Car = flexibleCar, CurrentBatteryPercentage = 0, TargetBatteryPercentage = 100, DepartureTime = null },
            new() { Car = urgentCar, CurrentBatteryPercentage = 0, TargetBatteryPercentage = 100, DepartureTime = baseDate.AddHours(2) },
        };

        var result = service.PlanForMultipleCars(chargeInfos, prices, numberOfChargers: 1);

        var urgentPlan = result.Single(r => r.Car.Id == 1);
        var flexiblePlan = result.Single(r => r.Car.Id == 2);

        Assert.NotNull(urgentPlan.Window);
        Assert.Equal(baseDate, urgentPlan.Window!.StartTime);

        Assert.NotNull(flexiblePlan.Window);
        Assert.NotEqual(baseDate, flexiblePlan.Window!.StartTime);
    }

    private static PriceRecord CreatePrice(DateTime baseDate, int hour, decimal price)
    {
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