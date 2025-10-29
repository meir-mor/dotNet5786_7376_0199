
namespace DO;


public enum typeOfShipment
{ 
    foot,
    bike,
    motorcycle,
    car
}

public enum OrderType
{
    food,
    flowers,
    clothing,
    dishes,
    general
}

public enum DeliveryEndType
{
    Supplied,
    RefusedByCustomer,
    Cancelled,
    CustomerNotFound,
    Failed
}

public enum ShipmentType
{
    foot,
    bike,
    motorcycle,
    car
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
