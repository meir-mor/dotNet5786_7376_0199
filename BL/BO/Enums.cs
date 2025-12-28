namespace BO;

public enum UserRole
{
    Manager,
    Courier
}

public enum OrderStatus
{
    Open,
    InProgress,
    Closed
}

public enum OrderType
{
    Food,
    Flowers,
    Clothing,
    Dishes,
    General
}

public enum ShipmentType
{
    Walk,
    Bicycle,
    Motorcycle,
    Car
}

public enum CourierListSortBy
{
    Id,
    FullName,
    StartWorkTime,
    TotalDeliveredOnTime,
    TotalDeliveredLate
}

/// <summary>
/// Filter keys for GetOrdersList.
/// Implementations map these keys to properties of BO.OrderInList.
/// </summary>
public enum OrderListFilterBy
{
    None,
    OrderType,
    CustomerFullName,

    OrderStatus,
    ShipmentType
}

/// <summary>
/// Sort keys for GetOrdersList.
/// </summary>
public enum OrderListSortBy
{
    OrderStatus,
    OrderOpenTime,
    AerialDistance,
    CustomerFullName
}

/// <summary>Sort options for open orders returned to couriers.</summary>
public enum OpenOrderSortBy
{
    ScheduleStatus,
    AerialDistance,
    OrderOpenTime
}

/// <summary>Sort options for closed deliveries history.</summary>
public enum ClosedDeliverySortBy
{
    ScheduleStatus,
    DeliveryEndTime,
    DeliveryType
}

/// <summary>
/// Time unit used by ForwardClock to advance the system clock by one step.
/// </summary>
public enum TimeUnit
{
    Minute,
    Hour,
    Day,
    Month,
    Year
}

public enum DeliveryEndType
{
    Supplied,
    RefusedByCustomer,
    Cancelled,
    CustomerNotFound,
    Failed
}