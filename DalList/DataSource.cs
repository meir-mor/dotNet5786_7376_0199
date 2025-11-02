namespace Dal;

internal static class DataSource
{
    internal static List<DO.Courier> Couriers { get; } = [];
    internal static List<DO.Order> Orders { get; } = [];
    internal static List<DO.Delivery> Deliveries { get; } = [];
}