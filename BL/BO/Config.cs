namespace BO;

public class Config
{
    public int ManagerId { get; set; }
    public DateTime Clock { get; set; }
    public TimeSpan RiskRange { get; set; }
    public string? ComponyAddress { get; set; }
    public string ManagerPassword { get; set; } = "";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? MaxDistance { get; set; }
    public double AverageCarSpeed { get; set; }
    public double AverageBikeSpeedBike { get; set; }
    public double AverageBicycleSpeed { get; set; }
    public double AverageWalkSpeed { get; set; }
    public TimeSpan MaxDeliveryTime { get; set; }
    public TimeSpan TimeToBeDispose { get; set; }
}