public enum UserRole
{
    Manager,
    Courier
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

    OrderStatus
    // add more keys as needed (e.g., Address, DateRange)
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