using EVChargePlanner.Domain;

namespace EVChargePlanner.Infrastructure;

public class PriceProviderFactory
{
    private readonly NorwayPriceProvider _norway;
    private readonly SpainPriceProvider _spain;

    public PriceProviderFactory(NorwayPriceProvider norway, SpainPriceProvider spain)
    {
        _norway = norway;
        _spain = spain;
    }

    public IPriceProvider GetProvider(string zone)
    {
        return zone.StartsWith("ES") ? _spain : _norway;
    }
}