using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using EVChargePlanner.Domain;

namespace EVChargePlanner.Infrastructure;

public class SpainPriceProvider : IPriceProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public SpainPriceProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<List<PriceRecord>> GetPricesAsync(DateOnly date, string zone)
    {
        var token = _configuration["Esios:Token"]
            ?? throw new InvalidOperationException("Missing Esios:Token configuration.");

        var startDate = date.ToString("yyyy-MM-ddT00:00:00");
        var endDate = date.ToString("yyyy-MM-ddT23:59:59");

        var url = $"https://api.esios.ree.es/indicators/1001?start_date={startDate}&end_date={endDate}";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Accept", "application/json; application/vnd.esios-api-v1+json");
        request.Headers.Add("Authorization", $"Token token=\"{token}\"");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadFromJsonAsync<EsiosResponse>();

        if (content?.Indicator?.Values == null)
        {
            return new List<PriceRecord>();
        }

        return content.Indicator.Values
            .Where(v => v.GeoId == 8741)
            .Select(v => new PriceRecord
            {
                TimeStart = v.Datetime,
                TimeEnd = v.Datetime.AddHours(1),
                PricePerKWh = v.Value / 1000m,
                Currency = "EUR",
                PriceZone = zone
            }).ToList();
    }

    private record EsiosResponse([property: JsonPropertyName("indicator")] EsiosIndicator? Indicator);
    private record EsiosIndicator([property: JsonPropertyName("values")] List<EsiosValue>? Values);
    private record EsiosValue(
        [property: JsonPropertyName("value")] decimal Value,
        [property: JsonPropertyName("datetime")] DateTime Datetime,
        [property: JsonPropertyName("geo_id")] int GeoId
    );
}