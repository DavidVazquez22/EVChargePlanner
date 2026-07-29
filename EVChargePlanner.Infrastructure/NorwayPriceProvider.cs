using System.Net.Http.Json;
using EVChargePlanner.Domain;

namespace EVChargePlanner.Infrastructure;

public class NorwayPriceProvider : IPriceProvider
{
    private readonly HttpClient _httpClient;

    public NorwayPriceProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<PriceRecord>> GetPricesAsync(DateOnly date, string zone)
    {
        var url = $"https://www.hvakosterstrommen.no/api/v1/prices/{date.Year}/{date.Month:D2}-{date.Day:D2}_{zone}.json";

        var response = await _httpClient.GetFromJsonAsync<List<NorwayPriceDto>>(url);

        if (response == null)
        {
            return new List<PriceRecord>();
        }

        return response.Select(dto => new PriceRecord
        {
            TimeStart = dto.TimeStart,
            TimeEnd = dto.TimeEnd,
            PricePerKWh = dto.NOK_per_kWh,
            Currency = "NOK",
            PriceZone = zone
        }).ToList();
    }

    private record NorwayPriceDto(decimal NOK_per_kWh, decimal EUR_per_kWh, DateTime TimeStart, DateTime TimeEnd);
}