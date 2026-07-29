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
}

public class ChargingWindow
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal TotalPricePerKWh { get; set; }
}