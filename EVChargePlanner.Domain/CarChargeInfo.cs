using EVChargePlanner.Domain;

public class CarChargeInfo
{
    public Car Car { get; set; } = null!;
    public int CurrentBatteryPercentage { get; set; }
    public int TargetBatteryPercentage { get; set; }
    public DateTime? DepartureTime { get; set; }
    public DateTime? ArrivalTime {get; set; }
}