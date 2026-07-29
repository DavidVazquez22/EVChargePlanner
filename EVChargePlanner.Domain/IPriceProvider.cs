namespace EVChargePlanner.Domain;

public interface IPriceProvider
{
    Task<List<PriceRecord>> GetPricesAsync(DateOnly date, string zone);
}