namespace EVChargePlanner.Domain;

public class CarModel
{
    public int Id { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public decimal BatteryCapacityKWh { get; set; }
    public decimal MaxChargingPowerKW { get; set; }
}