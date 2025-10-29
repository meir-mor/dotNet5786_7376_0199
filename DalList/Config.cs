namespace Dal;

internal static class Config
{
    // ======== מזהים ריצים (IDs) לדוגמה ========

    // מזהה התחלתי לישות Delivery
    private const int startDeliveryId = 0;
    private static int nextDeliveryId = startDeliveryId;
    internal static int NextDeliveryId { get => ++nextDeliveryId; }
       

    // מזהה התחלתי לישות Order
    private const int startOrderId = 0;
    private static int nextOrderId = startOrderId;
    internal static int NextOrderId { get => ++nextOrderId;}

    // ======== שדות תצורה נוספים ========

    internal static DateTime Clock { get; set; } = DateTime.Now;
    internal static int ManagerId { get; set; }
    internal static string ManagerPassword { get; set; } = "0";
    internal static string? ComponyAddress { get; set; }
    internal static double? Latitude { get; }// = ourFunctionToCalculate();
    internal static double? Longitude { get; }// = ourFunctionToCalculate();
    internal static double? MaxDistance { get; set; } = 100;
    internal static double AverageCarSpeed { get; set; }
    internal static double AverageBikeSpeedBike { get; set; }
    internal static double AverageBicycleSpeed { get; set; }
    internal static double AverageWalkSpeed { get; set; }
    internal static TimeSpan MaxDeliveryTime { get; set; }
    internal static TimeSpan RiskRange { get; set; } = TimeSpan.FromMinutes(30); 
    internal static TimeSpan TimeToBeDispose { get; set; }


    // ======== מתודה לאיפוס כל הערכים ========
    internal static void Reset()
    {
        // איפוס מזהים
        nextDeliveryId = startDeliveryId;
        nextOrderId = startOrderId;
        Clock = DateTime.Now;
        RiskRange = TimeSpan.FromMinutes(30);

        // איפוס שדות נוספים
        ManagerId = 0;
        ComponyAddress = "Bar Cochva 2 BneiBrak Israel";
    }
}