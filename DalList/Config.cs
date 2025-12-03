using DO;

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
    internal static int ManagerId { get; set; }
    internal static string ManagerPassword { get; set; } = "";
    internal static string? ComponyAddress { get; set; }
    internal static double? Latitude { get; set; } = 0;
    internal static double? Longitude { get; set; } = 0;
    internal static double? MaxDistance { get; set; } = 100;
    internal static double AverageCarSpeed { get; set; }
    internal static double AverageBikeSpeedBike { get; set; }
    internal static double AverageBicycleSpeed { get; set; }
    internal static double AverageWalkSpeed { get; set; }
    internal static TimeSpan MaxDeliveryTime { get; set; }
    internal static TimeSpan RiskRange { get; set; } = TimeSpan.FromMinutes(30);
    internal static TimeSpan TimeToBeDispose { get; set; }


    /// <summary>
    /// Resets all IDs and configuration values to initial defaults.
    /// </summary>
    internal static void Reset()
    {
        // איפוס מזהים
        _nextDeliveryId = StartDeliveryId;
        _nextOrderId = StartOrderId;
        Clock = DateTime.Now;
        RiskRange = TimeSpan.FromMinutes(30);

        ManagerId = 0;
        ComponyAddress = "Bar Cochva 2 BneiBrak Israel";
        Latitude = 32.09376490211114;
        Longitude = 34.82291411745883;
    }
}