namespace EVChargePlanner.Domain;

public class Charger
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MaxPowerKW { get; set; }
}