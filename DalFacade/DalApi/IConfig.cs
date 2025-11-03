namespace DalApi;

public interface IConfig
{
    /// <summary>
    /// add function in the future if needed
    /// </summary>
    /// DateTime Clock { get; set; }
    int ManagerId { get; set; }

    DateTime Clock { get; set; }
    TimeSpan RiskRange { get; set; }
    string ComponyAddress { get; set; }
    string ManagerPassword { get; set; }
    double? Latitude { get; }
    double? Longitude { get; }
    double? MaxDistance { get; set; }
    double AverageCarSpeed { get; set; }
    double AverageBikeSpeedBike { get; set; }
    double AverageBicycleSpeed { get; set; }
    double AverageWalkSpeed { get; set; }
    TimeSpan MaxDeliveryTime { get; set; }
    TimeSpan TimeToBeDispose { get; set; }

    int NextDeliveryId { get; }
    int NextOrderId { get; }
    void Reset();
}