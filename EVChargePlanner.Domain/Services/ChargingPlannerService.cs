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

            int boundaryIndex;
            decimal boundaryFraction = 0;

            if (info.DepartureTime.HasValue)
            {
                boundaryIndex = sortedPrices.FindIndex(p =>
                    p.TimeStart <= info.DepartureTime.Value && p.TimeEnd > info.DepartureTime.Value);

                if (boundaryIndex == -1)
                {
                    boundaryIndex = sortedPrices.FindIndex(p => p.TimeStart >= info.DepartureTime.Value);
                    if (boundaryIndex == -1) boundaryIndex = sortedPrices.Count;
                }
                else
                {
                    var hourStart = sortedPrices[boundaryIndex].TimeStart;
                    boundaryFraction = (decimal)(info.DepartureTime.Value - hourStart).TotalHours;
                }
            }
            else
            {
                boundaryIndex = sortedPrices.Count;
            }

            var canUsePartialHour = boundaryFraction > 0;
            var effectiveAvailableHours = boundaryIndex + (canUsePartialHour ? 1 : 0);
            var availableHours = Math.Min(hoursNeeded, effectiveAvailableHours);

            if (availableHours <= 0)
            {
                results.Add(new CarChargingPlan { Car = info.Car, Window = null });
                continue;
            }

            var searchLimit = boundaryIndex + (canUsePartialHour ? 1 : 0);
            var window = FindCheapestAvailableWindow(sortedPrices, occupancy, availableHours, searchLimit, numberOfChargers);

            if (window == null)
            {
                results.Add(new CarChargingPlan { Car = info.Car, Window = null });
                continue;
            }

            var (startIndex, totalPrice) = window.Value;
            var lastSlotIndex = startIndex + availableHours - 1;
            var usesPartialLastHour = canUsePartialHour && lastSlotIndex == boundaryIndex;

            for (int i = startIndex; i < startIndex + availableHours; i++)
            {
                occupancy[i]++;
            }

            DateTime endTime;
            decimal achievedEnergyKWh;
            var adjustedTotalPrice = totalPrice;

            if (usesPartialLastHour)
            {
                endTime = info.DepartureTime!.Value;
                achievedEnergyKWh = (availableHours - 1) * effectivePowerKW + effectivePowerKW * boundaryFraction;
                adjustedTotalPrice -= sortedPrices[boundaryIndex].PricePerKWh * (1 - boundaryFraction);
            }
            else
            {
                endTime = sortedPrices[lastSlotIndex].TimeEnd;
                achievedEnergyKWh = availableHours * effectivePowerKW;
            }

            var achievedPercentage = info.CurrentBatteryPercentage +
                (int)Math.Floor(achievedEnergyKWh / info.Car.BatteryCapacityKWh * 100);
            achievedPercentage = Math.Min(achievedPercentage, info.TargetBatteryPercentage);

            results.Add(new CarChargingPlan
            {
                Car = info.Car,
                Window = new ChargingWindow
                {
                    StartTime = sortedPrices[startIndex].TimeStart,
                    EndTime = endTime,
                    TotalPricePerKWh = adjustedTotalPrice,
                    AchievedBatteryPercentage = achievedPercentage,
                    IsPartialCharge = availableHours < hoursNeeded || usesPartialLastHour
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
    public int AchievedBatteryPercentage { get; set; }
    public bool IsPartialCharge { get; set; }
}

public class CarChargingPlan
{
    public Car Car { get; set; } = null!;
    public ChargingWindow? Window { get; set; }
}