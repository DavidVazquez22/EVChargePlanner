namespace EVChargePlanner.Domain;

public class Car
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal BatteryCapacityKWh { get; set; }
    public decimal MaxChargingPowerKW { get; set; }
    public string? ModelLabel { get; set; }

    public List<ChargingSession> ChargingSessions { get; set; } = new();
}