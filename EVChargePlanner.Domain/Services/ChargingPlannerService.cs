using EVChargePlanner.Domain;

namespace EVChargePlanner.Domain.Services;

public class ChargingPlannerService
{
    private static bool IsSlotAvailable(
        List<(DateTime Start, DateTime End)> reservedIntervals,
        DateTime start, DateTime end, int numberOfChargers)
    {
        var overlapping = reservedIntervals.Count(r => r.Start < end && r.End > start);
        return overlapping < numberOfChargers;
    }

    private static (DateTime Start, DateTime End, decimal TotalPrice)? FindCheapestAvailableSlot(
        List<PriceRecord> sortedPrices,
        List<(DateTime Start, DateTime End)> reservedIntervals,
        int minutesNeeded,
        DateTime earliestStart,
        DateTime deadline,
        int numberOfChargers)
    {
        const int stepMinutes = 15;
        (DateTime Start, DateTime End, decimal TotalPrice)? best = null;

        for (var candidateStart = earliestStart; candidateStart.AddMinutes(minutesNeeded) <= deadline; candidateStart = candidateStart.AddMinutes(stepMinutes))
        {
            var candidateEnd = candidateStart.AddMinutes(minutesNeeded);

            if (!IsSlotAvailable(reservedIntervals, candidateStart, candidateEnd, numberOfChargers))
            {
                continue;
            }

            var totalPrice = SumPriceOverInterval(sortedPrices, candidateStart, candidateEnd);

            if (best == null || totalPrice < best.Value.TotalPrice)
            {
                best = (candidateStart, candidateEnd, totalPrice);
            }
        }

        return best;
    }

    private static (DateTime Start, DateTime End, decimal TotalPrice)? FindBestPartialSlot(
        List<PriceRecord> sortedPrices,
        List<(DateTime Start, DateTime End)> reservedIntervals,
        DateTime earliestStart,
        DateTime deadline,
        int numberOfChargers)
    {
        const int stepMinutes = 15;
        (DateTime Start, DateTime End, decimal TotalPrice)? best = null;
        decimal bestDurationMinutes = 0;

        for (var candidateStart = earliestStart; candidateStart < deadline; candidateStart = candidateStart.AddMinutes(stepMinutes))
        {
            var maxEnd = deadline;

            for (var candidateEnd = maxEnd; candidateEnd > candidateStart; candidateEnd = candidateEnd.AddMinutes(-stepMinutes))
            {
                if (IsSlotAvailable(reservedIntervals, candidateStart, candidateEnd, numberOfChargers))
                {
                    var duration = (decimal)(candidateEnd - candidateStart).TotalMinutes;
                    var totalPrice = SumPriceOverInterval(sortedPrices, candidateStart, candidateEnd);

                    if (duration > bestDurationMinutes || (duration == bestDurationMinutes && (best == null || totalPrice < best.Value.TotalPrice)))
                    {
                        bestDurationMinutes = duration;
                        best = (candidateStart, candidateEnd, totalPrice);
                    }
                    break;
                }
            }
        }

        return best;
    }

    private static decimal SumPriceOverInterval(List<PriceRecord> sortedPrices, DateTime start, DateTime end)
    {
        decimal total = 0;
        var cursor = start;

        while (cursor < end)
        {
            var record = sortedPrices.FirstOrDefault(p => p.TimeStart <= cursor && p.TimeEnd > cursor);
            if (record == null) break;

            var segmentEnd = record.TimeEnd < end ? record.TimeEnd : end;
            var fractionOfHour = (decimal)(segmentEnd - cursor).TotalMinutes / 60m;
            total += record.PricePerKWh * fractionOfHour;

            cursor = segmentEnd;
        }

        return total;
    }

    public List<CarChargingPlan> PlanForMultipleCars(
        List<CarChargeInfo> chargeInfos,
        List<PriceRecord> prices,
        List<Charger> chargers,
        List<ChargingSession>? existingSessions = null)
    {
        var sortedPrices = prices.OrderBy(p => p.TimeStart).ToList();
        var reservedIntervals = new Dictionary<int, List<(DateTime Start, DateTime End)>>();

        foreach (var charger in chargers)
        {
            reservedIntervals[charger.Id] = new List<(DateTime Start, DateTime End)>();
        }

        if (existingSessions != null)
        {
            foreach (var session in existingSessions)
            {
                if (reservedIntervals.ContainsKey(session.ChargerId))
                {
                    reservedIntervals[session.ChargerId].Add((session.StartTime, session.EndTime));
                }
            }
        }

        var orderedInfos = chargeInfos
            .OrderBy(c => c.DepartureTime ?? DateTime.MaxValue)
            .ToList();

        var results = new List<CarChargingPlan>();
        var dayEnd = sortedPrices.Last().TimeEnd;

        foreach (var info in orderedInfos)
        {
            var energyNeededKWh = info.Car.BatteryCapacityKWh *
                (info.TargetBatteryPercentage - info.CurrentBatteryPercentage) / 100m;

            var effectivePowerKW = info.Car.MaxChargingPowerKW;
            var minutesNeeded = (int)Math.Ceiling((double)(energyNeededKWh / effectivePowerKW) * 60);

            var deadline = info.DepartureTime.HasValue && info.DepartureTime.Value < dayEnd
                ? info.DepartureTime.Value
                : dayEnd;
            var limitedByDataEnd = deadline == dayEnd;

           var globalEarliestStart = sortedPrices.First().TimeStart;
           var earliestStart = info.ArrivalTime.HasValue && info.ArrivalTime.Value > globalEarliestStart
                ? info.ArrivalTime.Value
                : globalEarliestStart;

            var assignment = FindCheapestChargerSlot(
                sortedPrices, reservedIntervals, chargers, minutesNeeded, earliestStart, deadline);

            if (assignment == null)
            {
                var fallback = FindBestPartialChargerSlot(sortedPrices, reservedIntervals, chargers, earliestStart, deadline);

                if (fallback == null)
                {
                    results.Add(new CarChargingPlan { Car = info.Car, Window = null });
                    continue;
                }

                var (fbCharger, fbStart, fbEnd, fbPrice) = fallback.Value;
                reservedIntervals[fbCharger.Id].Add((fbStart, fbEnd));

                var fbEnergyKWh = effectivePowerKW * (decimal)((fbEnd - fbStart).TotalMinutes / 60.0);
                var fbAchievedPct = info.CurrentBatteryPercentage +
                    (int)Math.Floor(fbEnergyKWh / info.Car.BatteryCapacityKWh * 100);

                results.Add(new CarChargingPlan
                {
                    Car = info.Car,
                    Window = new ChargingWindow
                    {
                        StartTime = fbStart,
                        EndTime = fbEnd,
                        TotalPricePerKWh = fbPrice,
                        AchievedBatteryPercentage = Math.Min(fbAchievedPct, info.TargetBatteryPercentage),
                        IsPartialCharge = true,
                        LimitedByDataEnd = limitedByDataEnd,
                        ChargerId = fbCharger.Id,
                        ChargerName = fbCharger.Name
                    }
                });
                continue;
            }

            var (charger, startTime, endTime, totalPrice) = assignment.Value;
            reservedIntervals[charger.Id].Add((startTime, endTime));

            var achievedEnergyKWh = effectivePowerKW * (decimal)((endTime - startTime).TotalMinutes / 60.0);
            var achievedPercentage = info.CurrentBatteryPercentage +
                (int)Math.Floor(achievedEnergyKWh / info.Car.BatteryCapacityKWh * 100);

            results.Add(new CarChargingPlan
            {
                Car = info.Car,
                Window = new ChargingWindow
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    TotalPricePerKWh = totalPrice,
                    AchievedBatteryPercentage = Math.Min(achievedPercentage, info.TargetBatteryPercentage),
                    IsPartialCharge = false,
                    LimitedByDataEnd = limitedByDataEnd,
                    ChargerId = charger.Id,
                    ChargerName = charger.Name
                }
            });
        }

        return results;
    }

    private static (Charger Charger, DateTime Start, DateTime End, decimal TotalPrice)? FindCheapestChargerSlot(
        List<PriceRecord> sortedPrices,
        Dictionary<int, List<(DateTime Start, DateTime End)>> reservedIntervals,
        List<Charger> chargers,
        int minutesNeeded,
        DateTime earliestStart,
        DateTime deadline)
    {
        const int stepMinutes = 15;
        (Charger Charger, DateTime Start, DateTime End, decimal TotalPrice)? best = null;

        for (var candidateStart = earliestStart; candidateStart.AddMinutes(minutesNeeded) <= deadline; candidateStart = candidateStart.AddMinutes(stepMinutes))
        {
            var candidateEnd = candidateStart.AddMinutes(minutesNeeded);

            foreach (var charger in chargers)
            {
                var isFree = reservedIntervals[charger.Id].All(r => r.Start >= candidateEnd || r.End <= candidateStart);
                if (!isFree) continue;

                var totalPrice = SumPriceOverInterval(sortedPrices, candidateStart, candidateEnd);

                if (best == null || totalPrice < best.Value.TotalPrice)
                {
                    best = (charger, candidateStart, candidateEnd, totalPrice);
                }
            }
        }

        return best;
    }

    private static (Charger Charger, DateTime Start, DateTime End, decimal TotalPrice)? FindBestPartialChargerSlot(
        List<PriceRecord> sortedPrices,
        Dictionary<int, List<(DateTime Start, DateTime End)>> reservedIntervals,
        List<Charger> chargers,
        DateTime earliestStart,
        DateTime deadline)
    {
        const int stepMinutes = 15;
        (Charger Charger, DateTime Start, DateTime End, decimal TotalPrice)? best = null;
        decimal bestDurationMinutes = 0;

        foreach (var charger in chargers)
        {
            for (var candidateStart = earliestStart; candidateStart < deadline; candidateStart = candidateStart.AddMinutes(stepMinutes))
            {
                for (var candidateEnd = deadline; candidateEnd > candidateStart; candidateEnd = candidateEnd.AddMinutes(-stepMinutes))
                {
                    var isFree = reservedIntervals[charger.Id].All(r => r.Start >= candidateEnd || r.End <= candidateStart);
                    if (!isFree) continue;

                    var duration = (decimal)(candidateEnd - candidateStart).TotalMinutes;
                    var totalPrice = SumPriceOverInterval(sortedPrices, candidateStart, candidateEnd);

                    if (duration > bestDurationMinutes || (duration == bestDurationMinutes && (best == null || totalPrice < best.Value.TotalPrice)))
                    {
                        bestDurationMinutes = duration;
                        best = (charger, candidateStart, candidateEnd, totalPrice);
                    }
                    break;
                }
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
    public int ChargerId { get; set; }
    public string ChargerName { get; set; } = string.Empty;
    public bool LimitedByDataEnd { get; set; }
}

public class CarChargingPlan
{
    public Car Car { get; set; } = null!;
    public ChargingWindow? Window { get; set; }
}