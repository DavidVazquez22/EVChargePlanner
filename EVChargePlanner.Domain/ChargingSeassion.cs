namespace EVChargePlanner.Domain;

public class ChargingSession
{
    public int Id { get; set; }
    public int CarId { get; set; }
    public Car? Car { get; set; }

    public int ChargerId { get; set; }
    public Charger? Charger { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal EnergyKWh { get; set; }
}