namespace DalList;

internal static class Config
{
    // ======== מזהים ריצים (IDs) לדוגמה ========

    // מזהה התחלתי לישות Courier
    private const int startCourierId = 0;
    private static int nextCouriertId = startCourierId;
    internal static int NextCourierId => ++nextCouriertId;

    // מזהה התחלתי לישות Delivery
    private const int startDeliveryId = 0;
    private static int nextDeliveryId = startDeliveryId;
    internal static int NextDeliveryId => ++nextDeliveryId;

    // מזהה התחלתי לישות Order
    private const int startOrderId = 0;
    private static int nextOrderId = startOrderId;
    internal static int NextEnrollmentId => ++nextOrderId;

    // ======== שדות תצורה נוספים ========
    private static string MenagerId { get; set; }
    internal static string ComponyAddress { get; set; }

    // ======== מתודה לאיפוס כל הערכים ========
    internal static void Reset()
    {
        // איפוס מזהים
        nextCouriertId = startCourierId;
        nextDeliveryId = startDeliveryId;
        nextOrderId = startOrderId;

        // איפוס שדות נוספים
        MenagerId = "049821633";
        ComponyAddress = "Bar Cochva 2 BneiBrak Israel";
    }
}