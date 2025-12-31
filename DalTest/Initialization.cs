using DalTest.Helpers;

namespace DalTest;

using DalApi;
using DO;

public static class Initialization
{
    private static IDal? s_dal;

    private static readonly Random s_rand = new();

    public static void Do()
    {
        s_dal = Factory.Get;

        Console.WriteLine("Reset Configuration values and List values...");
        s_dal.ResetDB();

        Console.WriteLine("Initializing list ...");
        CreateCourier(20);
        CreateOrder(50);
        CreateDelivery();
    }

    private static void CreateOrder(int count)
    {
        if (s_dal is null) return;

        for (var i = 0; i < count; i++)
        {
            var (address, _) = Mock.Addresses[i % Mock.Addresses.Length];

            var (lat, lon) = DistanceCalculator.GetCoordinates(address);

            if (lat is null || lon is null)
            {
                Console.WriteLine("Invalid coordinates. Order not created.");
                continue;
            }

            var status = i switch
            {
                < 20 => OrderStatus.Open,
                < 30 => OrderStatus.InProgress,
                _ => OrderStatus.Closed
            };

            var createdAt = s_dal.Config.Clock.AddMinutes(-s_rand.Next(60, 10080));

            var volume = s_rand.Next(1, 100);

            s_dal.Order?.Create(new Order(
                0,
                (OrderType)s_rand.Next(Enum.GetValues<OrderType>().Length),
                $"Package delivery for {Mock.CustomerName[i % Mock.CustomerName.Length]}",
                address,
                Mock.CustomerName[i % Mock.CustomerName.Length],
                $"054-{s_rand.Next(1000000, 9999999)}",
                s_rand.NextDouble() > 0.8,
                createdAt,
                status,
                lat ?? 0,
                lon ?? 0,
                volume
            ));
        }
    }

    private static void CreateCourier(int count)
    {
        if (s_dal == null) return;

        for (var i = 0; i < count; i++)
        {
            var id = s_rand.Next(200000000, 400000000);
            var name = Mock.CourierNames[i % Mock.CourierNames.Length];
            var email = $"{name.Replace(" ", ".").ToLower()}@gmail.com";
            var phone = $"052-{s_rand.Next(1000000, 9999999)}";

            var shipmentType = (ShipmentType)s_rand.Next(Enum.GetValues(typeof(ShipmentType)).Length);

            var isActive = s_rand.NextDouble() < 0.75;

            var startDate = s_dal.Config.Clock.AddDays(-s_rand.Next(30, 1000));

            double? maxDistance = shipmentType switch
            {
                ShipmentType.Walk => s_rand.Next(10, 60),
                ShipmentType.Bicycle => s_rand.Next(30, 160),
                ShipmentType.Motorcycle => s_rand.Next(100, 410),
                ShipmentType.Car => s_rand.Next(300, 1010),
                _ => null
            };

            if (s_rand.NextDouble() < 0.2)
                maxDistance = null;

            s_dal.Courier?.Create(new Courier(
                id,
                name,
                phone,
                email,
                "1234",
                isActive,
                ShipmentType: shipmentType,
                StartingWork: startDate,
                MaximumDistance: maxDistance
            ));
        }
    }

    private static void CreateDelivery()
    {
        if (s_dal is null) return;

        var orders = s_dal.Order.ReadAll()?.ToList() ?? [];
        var couriers = s_dal.Courier?.ReadAll()?.ToList() ?? [];
        if (orders.Count == 0 || couriers.Count == 0) return;

        // Separate orders by status to handle them correctly
        var inProgressOrders = orders.Where(o => o.Status == OrderStatus.InProgress).ToList();
        var closedOrders = orders.Where(o => o.Status == OrderStatus.Closed).ToList();

        // Dictionary to track the "Last Available Time" for each courier to prevent overlaps
        var courierBusyUntil = new Dictionary<int, DateTime>();
        foreach (var c in couriers) courierBusyUntil[c.Id] = c.StartingWork;

        // Process Closed Orders first (to populate history)
        foreach (var order in closedOrders)
        {
            AssignDelivery(order, couriers, courierBusyUntil, isFinished: true);
        }

        // Process In-Progress Orders
        foreach (var order in inProgressOrders)
        {
            AssignDelivery(order, couriers, courierBusyUntil, isFinished: false);
        }
    }

    private static void AssignDelivery(Order order, List<Courier> couriers, Dictionary<int, DateTime> busyTracker,
        bool isFinished)
    {
        // Calculate distance first to filter couriers
        var (_, shipmentType) = Mock.Addresses.FirstOrDefault(a => a.Address == order.Address);
        var dist = DistanceCalculator.CalculateDistance(s_dal!.Config.Latitude, s_dal.Config.Longitude, order.Latitude,
            order.Longitude, shipmentType);

        // Filter couriers: Active AND (No limit OR limit >= distance)
        var validCouriers = couriers.Where(c => c.IsActive && (c.MaximumDistance == null || c.MaximumDistance >= dist))
            .ToList();

        if (!validCouriers.Any()) return;

        var courier = validCouriers[s_rand.Next(validCouriers.Count)];

        // Start time must be after Order.CreatedAt AND after the Courier finished their last job
        var earliestStart = order.CreatedAt > busyTracker[courier.Id] ? order.CreatedAt : busyTracker[courier.Id];
        var startTime = earliestStart.AddMinutes(s_rand.Next(5, 30));

        DateTime? endTime = null;
        DeliveryEndType? endType = null;

        if (isFinished)
        {
            endTime = startTime.AddMinutes(s_rand.Next(20, 120));
            busyTracker[courier.Id] = endTime.Value;

            var roll = s_rand.NextDouble();
            endType = roll switch
            {
                < 0.80 => DeliveryEndType.Supplied,
                < 0.85 => DeliveryEndType.RefusedByCustomer,
                < 0.90 => DeliveryEndType.CustomerNotFound,
                < 0.95 => DeliveryEndType.Cancelled,
                _ => DeliveryEndType.Failed
            };
        }
        else
        {
            // For in-progress, we assume the courier is busy for at least 1 hour from now
            busyTracker[courier.Id] = startTime.AddHours(1);
        }

        s_dal.Delivery?.Create(new Delivery(
            0,
            order.Id,
            courier.Id,
            shipmentType,
            startTime,
            EndTime: endTime,
            ActualDistance: dist,
            DeliveryEndType: endType
        ));
    }
}