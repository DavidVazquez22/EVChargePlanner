using EVChargePlanner.Domain;
using EVChargePlanner.Domain.Services;
using Xunit;

namespace EVChargePlanner.Tests;

public class ChargingPlannerServiceTests
{

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

        var chargers = new List<Charger> { new() { Id = 1, Name = "Home Charger", MaxPowerKW = 11 } };
        var result = service.PlanForMultipleCars(chargeInfos, prices, chargers);

        var urgentPlan = result.Single(r => r.Car.Id == 1);
        var flexiblePlan = result.Single(r => r.Car.Id == 2);

        Assert.NotNull(urgentPlan.Window);
        Assert.Equal(baseDate, urgentPlan.Window!.StartTime);

        Assert.NotNull(flexiblePlan.Window);
        Assert.NotEqual(baseDate, flexiblePlan.Window!.StartTime);
    }

    [Fact]
    public void PlanForMultipleCars_WithDeadlineMidHour_EndsExactlyAtDeadline()
    {
        var service = new ChargingPlannerService();
        var baseDate = new DateTime(2026, 1, 1);

        var prices = new List<PriceRecord>
        {
            CreatePrice(baseDate, hour: 0, price: 1.0m),
            CreatePrice(baseDate, hour: 1, price: 1.0m),
            CreatePrice(baseDate, hour: 2, price: 1.0m),
        };

        var car = new Car
        {
            Id = 1,
            Name = "Test Car",
            BatteryCapacityKWh = 40,
            MaxChargingPowerKW = 10,
        };

        var deadline = baseDate.AddHours(2).AddMinutes(50);

        var chargeInfos = new List<CarChargeInfo>
        {
            new() { Car = car, CurrentBatteryPercentage = 0, TargetBatteryPercentage = 100, DepartureTime = deadline },
        };

        var chargers = new List<Charger> { new() { Id = 1, Name = "Home Charger", MaxPowerKW = 11 } };
        var result = service.PlanForMultipleCars(chargeInfos, prices, chargers);

        var plan = result.Single();

        Assert.NotNull(plan.Window);
        Assert.Equal(deadline, plan.Window!.EndTime);
        Assert.True(plan.Window.IsPartialCharge);
    }
    
    [Fact]
    public void PlanForMultipleCars_WithMinutePrecision_AllowsBackToBackSlotsWithoutOverlap()
    {
        var service = new ChargingPlannerService();
        var baseDate = new DateTime(2026, 1, 1);

        var prices = new List<PriceRecord>
        {
            CreatePrice(baseDate, hour: 0, price: 1.0m),
            CreatePrice(baseDate, hour: 1, price: 1.0m),
            CreatePrice(baseDate, hour: 2, price: 1.0m),
            CreatePrice(baseDate, hour: 3, price: 1.0m),
        };

        var carA = new Car { Id = 1, Name = "A", BatteryCapacityKWh = 10, MaxChargingPowerKW = 10 };
        var carB = new Car { Id = 2, Name = "B", BatteryCapacityKWh = 10, MaxChargingPowerKW = 10 };

        var chargeInfos = new List<CarChargeInfo>
        {
            new() { Car = carA, CurrentBatteryPercentage = 0, TargetBatteryPercentage = 100, DepartureTime = baseDate.AddHours(1).AddMinutes(15) },
            new() { Car = carB, CurrentBatteryPercentage = 0, TargetBatteryPercentage = 100, DepartureTime = baseDate.AddHours(2).AddMinutes(30) },
        };

        var chargers = new List<Charger> { new() { Id = 1, Name = "Home Charger", MaxPowerKW = 11 } };
        var result = service.PlanForMultipleCars(chargeInfos, prices, chargers);

        var planA = result.Single(r => r.Car.Id == 1);
        var planB = result.Single(r => r.Car.Id == 2);

        Assert.NotNull(planA.Window);
        Assert.NotNull(planB.Window);
        Assert.True(planA.Window!.EndTime <= planB.Window!.StartTime,
            "Car A's window should end before or exactly when Car B's window starts, no overlap.");
    }

    [Fact]
    public void PlanForMultipleCars_WithTwoChargers_AllowsSimultaneousCharging()
    {
        var service = new ChargingPlannerService();
        var baseDate = new DateTime(2026, 1, 1);

        var prices = new List<PriceRecord>
        {
            CreatePrice(baseDate, hour: 0, price: 1.0m),
            CreatePrice(baseDate, hour: 1, price: 1.0m),
        };

        var carA = new Car { Id = 1, Name = "A", BatteryCapacityKWh = 10, MaxChargingPowerKW = 10 };
        var carB = new Car { Id = 2, Name = "B", BatteryCapacityKWh = 10, MaxChargingPowerKW = 10 };

        var chargeInfos = new List<CarChargeInfo>
        {
            new() { Car = carA, CurrentBatteryPercentage = 0, TargetBatteryPercentage = 100, DepartureTime = baseDate.AddHours(1) },
            new() { Car = carB, CurrentBatteryPercentage = 0, TargetBatteryPercentage = 100, DepartureTime = baseDate.AddHours(1) },
        };

        var chargers = new List<Charger>
        {
            new() { Id = 1, Name = "Charger 1", MaxPowerKW = 11 },
            new() { Id = 2, Name = "Charger 2", MaxPowerKW = 11 },
        };
        var result = service.PlanForMultipleCars(chargeInfos, prices, chargers);

        var planA = result.Single(r => r.Car.Id == 1);
        var planB = result.Single(r => r.Car.Id == 2);

        Assert.NotNull(planA.Window);
        Assert.NotNull(planB.Window);
        Assert.False(planA.Window!.IsPartialCharge);
        Assert.False(planB.Window!.IsPartialCharge);
    }

    [Fact]
    public void PlanForMultipleCars_WhenNotEnoughTime_ChoosesLongestAvailableSlot()
    {
        var service = new ChargingPlannerService();
        var baseDate = new DateTime(2026, 1, 1);

        var prices = new List<PriceRecord>
        {
            CreatePrice(baseDate, hour: 0, price: 1.0m),
            CreatePrice(baseDate, hour: 1, price: 1.0m),
            CreatePrice(baseDate, hour: 2, price: 1.0m),
        };

        var car = new Car { Id = 1, Name = "Big Battery", BatteryCapacityKWh = 100, MaxChargingPowerKW = 10 };

        var deadline = baseDate.AddHours(1).AddMinutes(30);

        var chargeInfos = new List<CarChargeInfo>
        {
            new() { Car = car, CurrentBatteryPercentage = 0, TargetBatteryPercentage = 100, DepartureTime = deadline },
        };

        var chargers = new List<Charger> { new() { Id = 1, Name = "Home Charger", MaxPowerKW = 11 } };
        var result = service.PlanForMultipleCars(chargeInfos, prices, chargers);

        var plan = result.Single();

        Assert.NotNull(plan.Window);
        Assert.True(plan.Window!.IsPartialCharge);
        Assert.Equal(90, (plan.Window.EndTime - plan.Window.StartTime).TotalMinutes);
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