namespace DO;

public enum ShipmentType
{
    Walk,
    Bicycle,
    Motorcycle,
    Car,
    Aerial
}

public enum OrderType
{
    Food,
    Flowers,
    Clothing,
    Dishes,
    General
}

public enum DeliveryEndType
{
    Supplied,
    RefusedByCustomer,
    Cancelled,
    CustomerNotFound,
    Failed
}

public enum MainMenuOption
{
    Exit = 0,
    CourierMenu = 1,
    DeliveryMenu,
    OrderMenu,
    ConfigMenu,
    Initialization,
    ShowAllData,
    ResetAll
}

public enum EntityMenuOption
{
    Exit = 0,
    Create,
    Read,
    ReadAll,
    Update,
    Delete,
    DeleteAll
}

public enum ConfigMenuOption
{
    Exit = 0,
    AdvanceMinute,
    AdvanceHour,
    ShowClock,
    SetClock,
    Reset
}

public enum OrderStatus
{
    Open,
    InProgress,
    Closed
}