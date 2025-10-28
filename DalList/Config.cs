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
    internal static TimeSpan RiskRange { get; set; } = TimeSpan.FromMinutes(30); 
    internal static string ManagerId { get; set; }
    internal static string? ComponyAddress { get; set; }

    // ======== מתודה לאיפוס כל הערכים ========
    internal static void Reset()
    {
        // איפוס מזהים
        nextDeliveryId = startDeliveryId;
        nextOrderId = startOrderId;
        Clock = DateTime.Now;
        RiskRange = TimeSpan.FromMinutes(30);

        // איפוס שדות נוספים
        ManagerId = "049821633";
        ComponyAddress = "Bar Cochva 2 BneiBrak Israel";
    }
}