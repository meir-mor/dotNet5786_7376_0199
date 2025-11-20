
namespace BO;

public enum ShipmentType
{
    Walk,
    Bicycle,
    Motorcycle,
    Car
}
public enum OrderType
{
    Food,
    Flowers,
    Clothing,
    Dishes,
    General
}

public enum OrderStatus
{
    Open,
    InProgress,
    Closed
}

public enum DeliveryEndType
{
    Supplied,
    RefusedByCustomer,
    Cancelled,
    CustomerNotFound,
    Failed
}

public enum ScheduleStatus
{
    OnTime,
    AtRisk,
    Late
}
