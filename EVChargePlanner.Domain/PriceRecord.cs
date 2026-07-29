namespace EVChargePlanner.Domain;

public class PriceRecord
{
    public int Id { get; set; }
    public DateTime TimeStart { get; set; }
    public DateTime TimeEnd { get; set; }
    public decimal PricePerKWh { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string PriceZone { get; set; } = string.Empty;
}