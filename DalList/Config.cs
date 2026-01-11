using DO;
using System.Runtime.CompilerServices;

namespace Dal;

internal static class Config
{
    // Initial ID for Delivery entity
    private const int StartDeliveryId = 0;
    private static int _nextDeliveryId = StartDeliveryId;
    internal static int NextDeliveryId => ++_nextDeliveryId;


    // Initial ID for Order entity
    private const int StartOrderId = 0;
    private static int _nextOrderId = StartOrderId;
    internal static int NextOrderId => ++_nextOrderId;

    // Additional configuration fields

    internal static DateTime Clock { get; set; } = DateTime.Now;
    internal static int ManagerId { get; set; } = 1234;
    internal static string ManagerPassword { get; set; } = "1234";
    internal static string? ComponyAddress { get; set; } = "Bar Cochva 2 BneiBrak Israel";
    internal static double? Latitude { get; set; } = 32.09376490211114;
    internal static double? Longitude { get; set; } = 34.82291411745883;
    internal static double? MaxDistance { get; set; } = 100;
    internal static double AverageCarSpeed { get; set; } = 60;
    internal static double AverageBikeSpeedBike { get; set; } = 20;
    internal static double AverageBicycleSpeed { get; set; } = 15;
    internal static double AverageWalkSpeed { get; set; } = 5;
    internal static TimeSpan MaxDeliveryTime { get; set; } = TimeSpan.FromHours(1);
    internal static TimeSpan RiskRange { get; set; } = TimeSpan.FromMinutes(30);
    internal static TimeSpan TimeToBeDispose { get; set; } = TimeSpan.FromHours(1);


    /// <summary>
    /// Resets all IDs and configuration values to initial defaults.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Reset()
    {
        // איפוס מזהים
        _nextDeliveryId = StartDeliveryId;
        _nextOrderId = StartOrderId;
        Clock = DateTime.Now;
        ManagerId = 1234;
        ManagerPassword = "1234";
        ComponyAddress = "Bar Cochva 2 BneiBrak Israel";
        Latitude = 32.09376490211114;
        Longitude = 34.82291411745883;
        MaxDistance = 100;
        AverageCarSpeed = 60;
        AverageBikeSpeedBike = 20;
        AverageBicycleSpeed = 15;
        AverageWalkSpeed = 5;
        MaxDeliveryTime = TimeSpan.FromHours(10);
        TimeToBeDispose = TimeSpan.FromDays(365);
        RiskRange = TimeSpan.FromMinutes(30);
    }
}