using EVChargePlanner.Domain;

namespace EVChargePlanner.Domain.Services;

public class ChargingPlannerService
{
    public ChargingWindow? FindCheapestWindow(List<PriceRecord> prices, int hoursNeeded)
    {
        if (prices.Count < hoursNeeded || hoursNeeded <= 0)
        {
            return null;
        }

        var sortedPrices = prices.OrderBy(p => p.TimeStart).ToList();

        decimal currentWindowSum = 0;
        for (int i = 0; i < hoursNeeded; i++)
        {
            currentWindowSum += sortedPrices[i].PricePerKWh;
        }

        decimal bestSum = currentWindowSum;
        int bestStartIndex = 0;

        for (int i = hoursNeeded; i < sortedPrices.Count; i++)
        {
            currentWindowSum += sortedPrices[i].PricePerKWh;
            currentWindowSum -= sortedPrices[i - hoursNeeded].PricePerKWh;

            if (currentWindowSum < bestSum)
            {
                bestSum = currentWindowSum;
                bestStartIndex = i - hoursNeeded + 1;
            }
        }

        return new ChargingWindow
        {
            StartTime = sortedPrices[bestStartIndex].TimeStart,
            EndTime = sortedPrices[bestStartIndex + hoursNeeded - 1].TimeEnd,
            TotalPricePerKWh = bestSum
        };
    }


public List<CarChargingPlan> PlanForMultipleCars(
        List<CarChargeInfo> chargeInfos,
        List<PriceRecord> prices,
        int numberOfChargers)
    {
        var sortedPrices = prices.OrderBy(p => p.TimeStart).ToList();
        var occupancy = new int[sortedPrices.Count];

        var orderedInfos = chargeInfos
            .OrderBy(c => c.DepartureTime ?? DateTime.MaxValue)
            .ToList();

        var results = new List<CarChargingPlan>();

        foreach (var info in orderedInfos)
        {
            var energyNeededKWh = info.Car.BatteryCapacityKWh *
                (info.TargetBatteryPercentage - info.CurrentBatteryPercentage) / 100m;

            var effectivePowerKW = info.Car.MaxChargingPowerKW;
            var hoursNeeded = (int)Math.Ceiling(energyNeededKWh / effectivePowerKW);

            var deadlineIndex = info.DepartureTime.HasValue
                ? sortedPrices.FindIndex(p => p.TimeStart >= info.DepartureTime.Value)
                : sortedPrices.Count;

            if (deadlineIndex == -1)
            {
                deadlineIndex = sortedPrices.Count;
            }

            var window = FindCheapestAvailableWindow(
                sortedPrices, occupancy, hoursNeeded, deadlineIndex, numberOfChargers);

            if (window == null)
            {
                results.Add(new CarChargingPlan { Car = info.Car, Window = null });
                continue;
            }

            var (startIndex, totalPrice) = window.Value;

            for (int i = startIndex; i < startIndex + hoursNeeded; i++)
            {
                occupancy[i]++;
            }

            results.Add(new CarChargingPlan
            {
                Car = info.Car,
                Window = new ChargingWindow
                {
                    StartTime = sortedPrices[startIndex].TimeStart,
                    EndTime = sortedPrices[startIndex + hoursNeeded - 1].TimeEnd,
                    TotalPricePerKWh = totalPrice
                }
            });
        }

        return results;
    }

    private static (int StartIndex, decimal TotalPrice)? FindCheapestAvailableWindow(
        List<PriceRecord> sortedPrices, int[] occupancy, int hoursNeeded, int deadlineIndex, int numberOfChargers)
    {
        (int StartIndex, decimal TotalPrice)? best = null;

        for (int start = 0; start + hoursNeeded <= deadlineIndex; start++)
        {
            bool hasCapacity = true;
            decimal sum = 0;

            for (int i = start; i < start + hoursNeeded; i++)
            {
                if (occupancy[i] >= numberOfChargers)
                {
                    hasCapacity = false;
                    break;
                }
                sum += sortedPrices[i].PricePerKWh;
            }

            if (hasCapacity && (best == null || sum < best.Value.TotalPrice))
            {
                best = (start, sum);
            }
        }

        return best;
    }
}

public class ChargingWindow
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalPricePerKWh { get; set; }
}

public class CarChargingPlan
{
    public Car Car { get; set; } = null!;
    public ChargingWindow? Window { get; set; }
}