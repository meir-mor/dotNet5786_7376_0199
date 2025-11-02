namespace Dal;

using DalApi;

public class ConfigImplementation : IConfig
{
    public int ManagerId
    {
        get => Config.ManagerId;
        set => Config.ManagerId = value;
    }

    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }

    public TimeSpan RiskRange
    {
        get => Config.RiskRange;
        set => Config.RiskRange = value;
    }

    public string ComponyAddress
    {
        get => Config.ComponyAddress;
        set => Config.ComponyAddress = value;
    }

    public string ManagerPassword
    {
        get => Config.ManagerPassword;
        set => Config.ManagerPassword = value;
    }

    public double? Latitude => Config.Latitude;

    public double? Longitude => Config.Longitude;

    public double? MaxDistance
    {
        get => Config.MaxDistance;
        set => Config.MaxDistance = value;
    }

    public double AverageCarSpeed
    {
        get => Config.AverageCarSpeed;
        set => Config.AverageCarSpeed = value;
    }

    public double AverageBikeSpeedBike
    {
        get => Config.AverageBikeSpeedBike;
        set => Config.AverageBikeSpeedBike = value;
    }

    public double AverageBicycleSpeed
    {
        get => Config.AverageBicycleSpeed;
        set => Config.AverageBicycleSpeed = value;
    }

    public double AverageWalkSpeed
    {
        get => Config.AverageWalkSpeed;
        set => Config.AverageWalkSpeed = value;
    }

    public TimeSpan MaxDeliveryTime
    {
        get => Config.MaxDeliveryTime;
        set => Config.MaxDeliveryTime = value;
    }

    public TimeSpan TimeToBeDispose
    {
        get => Config.TimeToBeDispose;
        set => Config.TimeToBeDispose = value;
    }

    public int NextDeliveryId => Config.NextDeliveryId;

    public int NextOrderId => Config.NextOrderId;

    public void Reset()
    {
        Config.Reset();
    }
}