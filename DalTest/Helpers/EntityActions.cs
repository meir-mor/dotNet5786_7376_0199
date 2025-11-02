using Dal;
using DO;
using static DalTest.Program;
namespace DalTest.Helpers;

internal static class EntityActions
{
    private static readonly ConfigImplementation ConfigImplementation = new ConfigImplementation();
    //TO DO: 
    
    /// <summary>
    /// Creates a new entity from console input and adds it to the DAL.
    /// </summary>
    public static void CreateEntity(dynamic dalEntity, string entityName)
    {
        var newEntity = GetFromConsole(entityName);
        dalEntity.Create(newEntity);
        Console.WriteLine($@"{entityName} added successfully.");
    }

    /// <summary>
    /// Updates an existing entity by reading its ID and new data from console.
    /// </summary>
    public static void UpdateEntity(dynamic dalEntity, string entityName)
    {
        Console.Write($@"Enter ID for {entityName}: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Invalid ID!");
            return;
        }
        var newEntity = dalEntity.Read(id);
        if (newEntity is null)
        {
            Console.WriteLine($@"{entityName} not found.");
            return;
        }
        Console.WriteLine($@"Update {newEntity}.");
        var EntityFronConsole = GetFromConsole(newEntity);
        dalEntity.Update(EntityFronConsole);
        Console.WriteLine($@"{EntityFronConsole} updated successfully.");
    }

    /// <summary>
    /// Reads and displays an entity by ID from the DAL.
    /// </summary>
    public static void ReadEntity(dynamic dalEntity)
    {
        Console.Write(@"Enter ID: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Invalid ID!");
            return;
        }
        var entity = dalEntity.Read(id);
        Console.WriteLine(entity ?? "Not found.");
    }

    /// <summary>
    /// Reads and displays all entities from the DAL.
    /// </summary>
    public static void ReadAllEntities(dynamic dalEntity)
    {
        var list = dalEntity.ReadAll();
        foreach (var item in list)
            Console.WriteLine(item);
    }

    /// <summary>
    /// Deletes an entity by ID from the DAL.
    /// </summary>
    public static void DeleteEntity(dynamic dalEntity)
    {
        Console.Write(@"Enter ID to delete: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Invalid ID!");
            return;
        }
        dalEntity.Delete(id);
        Console.WriteLine("Entity deleted.");
    }

    public static void ShowAllData()
    {
        Console.WriteLine(@"=== All Data ===");
        Console.WriteLine("Couriers:");
        ReadAllEntities(s_dalCourier);
        Console.WriteLine("\nDeliveries:");
        ReadAllEntities(s_dalDelivery);
        Console.WriteLine("\nOrders:");
        ReadAllEntities(s_dalOrder);
    }

    public static void ResetAll()
    {
        s_dalCourier?.DeleteAll();
        s_dalDelivery?.DeleteAll();
        s_dalOrder?.DeleteAll();
        s_dalConfig?.Reset();
        Console.WriteLine(@"All data reset successfully.");
    }

    /// <summary>
    /// Gets entity data from console based on entity name.
    /// </summary>
    private static dynamic? GetFromConsole(string entityName)
    {
        switch(entityName)
        {
            case "Courier":
                return CreateCourier();
            case "Order":
                return CreateOrder();
            case "Delivery":
                return CreateDelivery();
            default:
                return null;
        }
    }
    
    private static Courier CreateCourier()
    {
        Console.Write("Enter Courier ID: ");
        var id = int.TryParse(Console.ReadLine(), out var tmpId) ? tmpId : 0;

        Console.Write("Enter Name: ");
        var name = Console.ReadLine() ?? "";

        Console.Write("Enter Phone: ");
        var phone = Console.ReadLine() ?? "";

        Console.Write("Enter Email: ");
        var email = Console.ReadLine() ?? "";

        Console.Write("Enter Password: ");
        var password = Console.ReadLine() ?? "";

        Console.Write("Is Active? (true/false): ");
        var isActive = bool.TryParse(Console.ReadLine(), out var tmpActive) && tmpActive;

        Console.Write("Enter Maximum Distance (or leave empty): ");
        double? maxDistance = double.TryParse(Console.ReadLine(), out var tmpDist) ? tmpDist : null;

        Console.Write("Enter Type of Shipment (number): ");
        var type = Enum.TryParse<typeOfShipment>(Console.ReadLine(), out var tmpType) ? tmpType : default;

        Console.Write("Enter Starting Work Date (yyyy-MM-dd): ");

        return new Courier(id, name, phone, email, password, isActive, maxDistance, type, ConfigImplementation.Clock);
    }

    private static Order CreateOrder()
    {
        var orderId = ConfigImplementation.NextOrderId;

        Console.Write("Enter Order Type (number): ");
        var type = Enum.TryParse<OrderType>(Console.ReadLine(), out var tmpType) ? tmpType : default;

        Console.Write("Enter Description: ");
        var description = Console.ReadLine();

        Console.Write("Enter Address: ");
        var address = Console.ReadLine() ?? "";

        Console.Write("Enter Customer Name: ");
        var customerName = Console.ReadLine() ?? "";

        Console.Write("Enter Customer Phone: ");
        var customerPhone = Console.ReadLine() ?? "";

        Console.Write("Is Fragile? (true/false): ");
        var isFragile = bool.TryParse(Console.ReadLine(), out var tmpFragile) && tmpFragile;
        
        Console.Write("Enter Longitude: ");
        double.TryParse(Console.ReadLine(), out var lon);
        
        Console.Write("Enter Latitude: ");
        double.TryParse(Console.ReadLine(), out var lat);

        return new Order(orderId, type, description, address, customerName, customerPhone, isFragile, ConfigImplementation.Clock, OrderStatus.Open, lat, lon);
    }

    private static Delivery CreateDelivery()
    {
        var deliveryId = ConfigImplementation.NextDeliveryId;

        Console.Write("Enter Order ID: ");
        var orderId = int.TryParse(Console.ReadLine(), out var tmpOrderId) ? tmpOrderId : 0;
       
        Console.Write("Enter Courier ID: ");
        var courierId = int.TryParse(Console.ReadLine(), out var tmpCourierId) ? tmpCourierId : 0;

        Console.Write("Enter Shipment Type (number): ");
        var shipmentType = Enum.TryParse<ShipmentType>(Console.ReadLine(), out var tmpShipment) ? tmpShipment : default;
        
        Console.Write("Enter Actual Distance (or leave empty): ");
        double? actualDistance = double.TryParse(Console.ReadLine(), out var tmpDist) ? tmpDist : null;
        
        return new Delivery(deliveryId, orderId, courierId, shipmentType, ConfigImplementation.Clock, actualDistance, null, DateTime.MinValue);
    }

}
