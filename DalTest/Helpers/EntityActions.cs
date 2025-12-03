using Dal;
using DalApi;
using DO;

namespace DalTest.Helpers;

internal static class EntityActions
{
    /// <summary>
    /// Creates a new entity from console input and adds it to the DAL.
    /// </summary>
    public static void CreateEntity<T>(ICrud<T> dalEntity, string entityName, IDal dal) where T : class
    {
        if (GetFromConsole(dal, entityName) is not T newEntity)
        {
            Console.WriteLine($"Failed to create {entityName}: invalid input.");
            return;
        }

        dalEntity.Create(newEntity);
        Console.WriteLine($@"{newEntity} added successfully.");
    }

    /// <summary>
    /// Updates an existing entity by reading its ID and new data from console.
    /// </summary>
    public static void UpdateEntity<T>(ICrud<T> dalEntity, string entityName, IDal dal) where T : class
    {
        Console.Write($@"Enter ID for {entityName}: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Invalid ID!");
            return;
        }

        var existingEntity = dalEntity.Read(id);
        if (existingEntity is null)
        {
            Console.WriteLine($@"{entityName} not found.");
            return;
        }

        Console.WriteLine($@"Update {existingEntity}.");
        if (GetFromConsole(dal, entityName) is not T updatedEntity)
        {
            Console.WriteLine("Invalid data. Update cancelled.");
            return;
        }

        dalEntity.Update(updatedEntity);
        Console.WriteLine($@"{updatedEntity} updated successfully.");
    }

    /// <summary>
    /// Reads and displays an entity by ID from the DAL.
    /// </summary>
    public static void ReadEntity<T>(ICrud<T> dalEntity) where T : class
    {
        Console.Write(@"Enter ID: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Invalid ID!");
            return;
        }

        var entity = dalEntity.Read(id);
        Console.WriteLine(entity is null ? "Not found." : entity);
    }

    /// <summary>
    /// Reads and displays all entities from the DAL.
    /// </summary>
    public static void ReadAllEntities<T>(ICrud<T> dalEntity) where T : class
    {
        var list = dalEntity.ReadAll();
        foreach (var item in list)
            Console.WriteLine(item);
    }

    /// <summary>
    /// Deletes an entity by ID from the DAL.
    /// </summary>
    public static void DeleteEntity<T>(ICrud<T> dalEntity) where T : class
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

    public static void ShowAllData(IDal dal)
    {
        Console.WriteLine(@"=== All Data ===");
        Console.WriteLine("Couriers:");
        ReadAllEntities(dal.Courier);
        Console.WriteLine("\nDeliveries:");
        ReadAllEntities(dal.Delivery);
        Console.WriteLine("\nOrders:");
        ReadAllEntities(dal.Order);
    }

    /// <summary>
    /// Gets entity data from console based on entity name.
    /// </summary>
    private static object? GetFromConsole(IDal dal, string entityName)
    {
        return entityName switch
        {
            "Courier" => CreateCourier(dal),
            "Order" => CreateOrder(dal),
            "Delivery" => CreateDelivery(dal),
            _ => null
        };
    }

    private static Courier CreateCourier(IDal dal)
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
        var type = Enum.TryParse<ShipmentType>(Console.ReadLine(), out var tmpType) ? tmpType : default;

        return new Courier(id, name, phone, email, password, isActive, maxDistance, type, dal.Config.Clock);
    }

    private static Order CreateOrder(IDal dal)
    {
        var orderId = dal.Config.NextOrderId;

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

        var (lat, lon) = DistanceCalculator.GetCoordinates(address);
        
        if (lat is null || lon is null)
        {
            Console.WriteLine("Invalid coordinates. Order not created.");
            return null;
        }

        return new Order(orderId, type, description, address, customerName, customerPhone, isFragile,
            dal.Config.Clock, OrderStatus.Open, lat ?? 0, lon ?? 0);
    }

    private static Delivery CreateDelivery(IDal dal)
    {
        var deliveryId = dal.Config.NextDeliveryId;

        Console.Write("Enter Order ID: ");
        var orderId = int.TryParse(Console.ReadLine(), out var tmpOrderId) ? tmpOrderId : 0;

        Console.Write("Enter Courier ID: ");
        var courierId = int.TryParse(Console.ReadLine(), out var tmpCourierId) ? tmpCourierId : 0;

        Console.Write("Enter Shipment Type (number): ");
        var shipmentType = Enum.TryParse<ShipmentType>(Console.ReadLine(), out var tmpShipment) ? tmpShipment : default;

        var order = dal.Order.Read(d => d.Id == tmpOrderId);
        
        var actualDistance = DistanceCalculator.CalculateDistance(dal.Config.Latitude, dal.Config.Longitude, order.Latitude, order.Longitude, tmpShipment);
        
        return new Delivery(deliveryId, orderId, courierId, shipmentType, dal.Config.Clock, actualDistance,
            null, DateTime.MinValue);
    }
}