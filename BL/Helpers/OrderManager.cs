using DalApi;
using DalTest.Helpers;
using DO;
using Factory = DalApi.Factory;

namespace Helpers;

internal static class OrderManager
{
    internal static ObserverManager Observers = new(); //stage 5 

    private static IDal s_dal = Factory.Get;

    /// <summary>
    /// Return all orders mapped to BO.Order (extension methods usage).
    /// </summary>
    internal static IEnumerable<BO.Order> GetAllOrders(Func<BO.Order, bool>? filter = null)
    {
        try
        {
            var orders = s_dal.Order.ReadAll()
                .Where(o => o != null) // extension 1
                .Select(o =>
                {
                    var bo = MapDoToBo(o); // extension 2 usage in chain
                    // populate deliveries for each order (extension 3 + 4 below)
                    bo.Deliveries = s_dal.Delivery.ReadAll(d => d.OrderId == o.Id)
                        .Select(d => new BO.DeliveryPerOrderInList
                        {
                            DeliveryId = d.Id,
                            CourierId = d.CourierId == 0 ? null : (int?)d.CourierId,
                            CourierName = s_dal.Courier.Read(d.CourierId) is DO.Courier c ? c.Name : string.Empty,
                            ShipmentType = (BO.ShipmentType)(int)d.ShipmentType,
                            DeliveryStartTime = d.StartTime,
                            DeliveryEndType = d.DeliveryEndType is null
                                ? null
                                : (BO.DeliveryEndType?)(int)d.DeliveryEndType,
                            DeliveryEndTime = d.EndTime is DateTime time && time != DateTime.MinValue ? d.EndTime : null
                        })
                        .ToList(); // extension ToList
                    return bo;
                })
                .OrderByDescending(o => o.OrderOpenTime)
                .ToList();

            return filter == null ? orders : orders.Where(filter);
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: error while reading orders", ex);
        }
    }


    /// <summary>
    /// Get single order by id, mapping DO -> BO and including deliveries (query syntax: grouping example).
    /// </summary>
    internal static BO.Order? GetOrderById(int id)
    {
        try
        {
            var doOrder = s_dal.Order.Read(id);
            if (doOrder == null) return null;

            var bo = MapDoToBo(doOrder);

            // query-syntax: group deliveries by courier for this order (demonstrates grouping + select new)
            var deliveries =
                from d in s_dal.Delivery.ReadAll()
                where d.OrderId == id
                group d by d.CourierId
                into g // grouping
                orderby g.Key
                select new
                {
                    CourierId = g.Key,
                    Items = g.Select(x => new BO.DeliveryPerOrderInList
                    {
                        DeliveryId = x.Id,
                        CourierId = x.CourierId,
                        CourierName = s_dal.Courier.Read(x.CourierId)?.Name,
                        ShipmentType = (BO.ShipmentType)(int)x.ShipmentType,
                        DeliveryStartTime = x.StartTime,
                        DeliveryEndType = x.DeliveryEndType is null
                            ? null
                            : (BO.DeliveryEndType?)(int)x.DeliveryEndType,
                        DeliveryEndTime = x.EndTime is DateTime time && time != DateTime.MinValue ? x.EndTime : null
                    }).ToList()
                };

            // flatten grouped results to a single list of deliveries
            bo.Deliveries = deliveries.SelectMany(g => g.Items).ToList();

            return bo;
        }
        catch (BO.BlException ex)
        {
            throw new BO.BlUnexpectedException($"DAL: error while reading order {id}", ex);
        }
    }

    /// <summary>
    /// Create order (maps BO -> DO).
    /// </summary>
    internal static void CreateOrder(BO.Order order)
    {
        try
        {
            var (lat, lon) = DistanceCalculator.GetCoordinates(order.FullAddress);
            if (lat is null || lon is null)
            {
                throw new BO.BlUnexpectedException("DAL: invalid coordinates");
            }

            var doOrder = new DO.Order(
                0,
                (DO.OrderType)(int)order.OrderType,
                order.Description,
                order.FullAddress,
                order.CustomerFullName,
                order.CustomerPhone,
                false, // BO doesn't expose IsFragile; default false
                order.OrderOpenTime,
                (DO.OrderStatus)order.OrderStatus,
                lat ?? 0,
                lon ?? 0,
                order.Volume
            );

            s_dal.Order.Create(doOrder);
            Observers.NotifyItemUpdated(doOrder.Id);
            Observers.NotifyListUpdated();
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("DAL: order already exists", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: unexpected error while creating order", ex);
        }
    }

    /// <summary>
    /// Update order (maps BO -> DO).
    /// </summary>
    internal static void UpdateOrder(BO.Order bo)
    {
        try
        {
            var existing = s_dal.Order.Read(bo.Id)
                           ?? throw new BO.BlDoesNotExistException($"Order {bo.Id} not found");

            if (existing.Status != DO.OrderStatus.Open)
                throw new BO.BlInvalidOperationException("Only open orders can be updated");

            var doOrder = new DO.Order(
                bo.Id,
                (DO.OrderType)(int)bo.OrderType,
                bo.Description,
                bo.FullAddress,
                bo.CustomerFullName,
                bo.CustomerPhone,
                false,
                existing.CreatedAt,
                existing.Status,
                bo.Latitude,
                bo.Longitude,
                bo.Volume
            );

            s_dal.Order.Update(doOrder);

            Observers.NotifyItemUpdated(bo.Id);
            Observers.NotifyListUpdated();
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"DAL: order {bo.Id} not found", ex);
        }
    }

    /// <summary>
    /// Update order (maps BO -> DO).
    /// </summary>
    /*internal static void UpdateOrder(BO.Order bo)
    {
        try
        {
            var doOrder = new DO.Order(
                bo.Id,
                (DO.OrderType)(int)bo.OrderType,
                bo.Description,
                bo.FullAddress,
                bo.CustomerFullName,
                bo.CustomerPhone,
                false,
                bo.OrderOpenTime,
                bo.OrderStatus,
                bo.Latitude,
                bo.Longitude
            );

            s_dal.Order.Update(doOrder);
            Observers.NotifyItemUpdated(bo.Id);
            Observers.NotifyListUpdated();
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"DAL: order {bo.Id} not found", ex);
        }
    }*/

    /// <summary>
    /// Delete order by id.
    /// </summary>
    internal static void DeleteOrder(int id)
    {
        try
        {
            var exists = s_dal.Order.ReadAll().Any(o => o.Id == id);
            if (!exists) throw new BO.BlException($"DAL: order {id} not found");

            s_dal.Order.Delete(id);
            Observers.NotifyItemUpdated(id);
            Observers.NotifyListUpdated();
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"DAL: order {id} does not exist", ex);
        }
    }

    internal static void PeriodicOrdersUpdates()
    {
        var list = s_dal.Order.ReadAll().ToList();
        bool updated = false;
        foreach (var doOrder in list)
        {
            //if student study for more than MaxRange years
            //then student should be automatically updated to 'not active'
            if (AdminManager.Now - doOrder.CreatedAt >= s_dal.Config.MaxDeliveryTime)
            {
                s_dal.Order.Update(doOrder with { Status = OrderStatus.Closed });
                updated = true;
                Observers.NotifyItemUpdated(doOrder.Id);
            }

            if (updated)
                Observers.NotifyListUpdated();
        }
    }

    private static BO.Order MapDoToBo(DO.Order o)
    {
        return new BO.Order
        {
            Id = o.Id,
            OrderType = (BO.OrderType)(int)o.Type,
            Description = o.Description,
            FullAddress = o.Address,
            Latitude = o.Latitude,
            Longitude = o.Longitude,
            AerialDistance = DistanceCalculator.CalculateDistance(s_dal.Config.Latitude, s_dal.Config.Longitude,
                o.Latitude, o.Longitude, DO.ShipmentType.Aerial), // computed elsewhere if needed
            CustomerFullName = o.CustomerName,
            CustomerPhone = o.CustomerPhone,
            Volume = o.Volume,
            OrderOpenTime = o.CreatedAt,
            ExpectedDeliveryTime = DateTime.MinValue,
            MaxDeliveryTime = TimeSpan.Zero,
            OrderStatus = (BO.OrderStatus)(int)o.Status,
            ScheduleStatus = DO.ScheduleStatus.OnTime,
            TimeRemaining = TimeSpan.Zero,
            Deliveries = new List<BO.DeliveryPerOrderInList>()
        };
    }

    public static int[,] GetOrdersCountsSummary(int requesterId)
    {
        try
        {
            var cfg = AdminManager.GetConfig();
            var now = AdminManager.Now;
            var riskRange = cfg.RiskRange;

            // get all orders
            var orders = s_dal.Order.ReadAll().ToList();

            // produce pairs of (OrderStatus, ScheduleStatus)
            var pairs = orders.Select(o =>
            {
                var maxDelivery = o.CreatedAt + cfg.MaxDeliveryTime;
                var timeRemaining = maxDelivery - now;
                var schedule = ComputeScheduleStatus(timeRemaining, riskRange);
                var orderStatus = (DO.OrderStatus)(int)o.Status;
                return new { OrderStatus = orderStatus, Schedule = schedule };
            });

            // group by both fields and count
            var groups = pairs
                .GroupBy(p => new { p.OrderStatus, p.Schedule })
                .Select(g => new { g.Key.OrderStatus, g.Key.Schedule, Count = g.Count() })
                .ToList();

            var orderStatusCount = Enum.GetValues(typeof(DO.OrderStatus)).Length;
            var scheduleCount = Enum.GetValues(typeof(DO.ScheduleStatus)).Length;

            var result = new int[orderStatusCount, scheduleCount];

            foreach (var g in groups) result[(int)g.OrderStatus, (int)g.Schedule] = g.Count;

            return result;
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: error while computing orders summary", ex);
        }
    }

    public static void CancelOrder(int orderId)
    {
        try
        {
            var now = AdminManager.Now;
            var order = s_dal.Order.Read(orderId) ?? throw new BO.BlException($"Order {orderId} not found");

            var status = order.Status;
            if (status == DO.OrderStatus.Open)
            {
                // create dummy delivery with start==end == now, cancelled, courier 0
                var dummy = new DO.Delivery(
                    0,
                    orderId,
                    0,
                    DO.ShipmentType.Motorcycle,
                    now,
                    null,
                    DO.DeliveryEndType.Cancelled,
                    now
                );
                s_dal.Delivery.Create(dummy);
            }
            else if (status == DO.OrderStatus.InProgress)
            {
                // find active delivery for this order (EndTime null)
                var delivery = s_dal.Delivery
                                   .ReadAll(d =>
                                       d.OrderId == orderId && (d.EndTime == null || d.DeliveryEndType == null))
                                   .FirstOrDefault()
                               ?? throw new BO.BlException($"No active delivery found for order {orderId}");
                var updated = delivery with { EndTime = now, DeliveryEndType = DO.DeliveryEndType.Cancelled };
                s_dal.Delivery.Update(updated);
            }
            else
            {
                throw new BO.BlInvalidOperationException($"Order {orderId} cannot be cancelled (status {status})");
            }

            Observers.NotifyItemUpdated(orderId);
            Observers.NotifyListUpdated();
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("DAL: entity not found", ex);
        }
        catch (DO.DalNullException ex)
        {
            throw new BO.BlInvalidOperationException("DAL: null", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlDalFileException("DAL: xml", ex);
        }
    }

    public static void CompleteDelivery(int requesterId, int courierId, int deliveryId)
    {
        try
        {
            var now = AdminManager.Now;
            var delivery = s_dal.Delivery.Read(deliveryId) ??
                           throw new BO.BlDoesNotExistException($"Delivery {deliveryId} not found");

            if (delivery.CourierId != courierId)
                throw new BO.BlInvalidOperationException("Requester is not the courier of this delivery");

            var order = s_dal.Order.Read(delivery.OrderId);
            if (order is null || order.Status != DO.OrderStatus.InProgress)
                throw new BO.BlException("Order is not in progress");

            // update delivery end and type
            var updated = delivery with { EndTime = now, DeliveryEndType = DO.DeliveryEndType.Supplied };
            s_dal.Delivery.Update(updated);
            s_dal.Order.Update(order with { Status = DO.OrderStatus.Closed, });
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("DAL: entity not found", ex);
        }
    }

    public static void AssignOrderToCourier(int requesterId, int courierId, int orderId)
    {
        try
        {
            var now = AdminManager.Now;
            var order = s_dal.Order.Read(orderId) ?? throw new BO.BlException($"Order {orderId} not found");

            if (order.Status != DO.OrderStatus.Open)
                throw new BO.BlException("Order is not open for assignment");

            // ensure no active delivery exists for this order
            var active = s_dal.Delivery
                .ReadAll(d => d.OrderId == orderId && (d.EndTime == null || d.DeliveryEndType == null))
                .FirstOrDefault();
            if (active != null)
                throw new BO.BlException("Order already has an active delivery");

            var distance = DistanceCalculator.CalculateDistance(s_dal.Config.Latitude, s_dal.Config.Longitude,
                order.Latitude, order.Longitude, DO.ShipmentType.Motorcycle);

            // create delivery (use default ShipmentType - can be improved)
            var delivery = new DO.Delivery(
                0,
                orderId,
                courierId,
                DO.ShipmentType.Motorcycle,
                now,
                distance,
                null,
                null
            );

            s_dal.Delivery.Create(delivery);

            // Optionally update order status to InProgress
            var updatedOrder = order with { Status = DO.OrderStatus.InProgress };
            s_dal.Order.Update(updatedOrder);
            Observers.NotifyItemUpdated(orderId);
            Observers.NotifyListUpdated();
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("DAL: entity not found", ex);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("DAL: already exists", ex);
        }
    }

    public static IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesForCourier(int requesterId, int courierId,
        BO.ShipmentType? typeFilter, BO.ClosedDeliverySortBy? sortBy)
    {
        try
        {
            var cfg = AdminManager.GetConfig();
            var now = AdminManager.Now;
            var riskRange = cfg.RiskRange;

            var query = s_dal.Delivery.ReadAll(d => d.CourierId == courierId && d.DeliveryEndType != null);

            var list = query
                .Where(d => typeFilter == null || d.ShipmentType == (DO.ShipmentType)(int)typeFilter.Value)
                .Select(d =>
                {
                    var order = s_dal.Order.Read(d.OrderId);
                    var orderType = order is null
                        ? DO.OrderType.General
                        : (DO.OrderType)(int)order.Type;
                    var fullAddress = order is null ? string.Empty : order.Address;
                    var handling = d.EndTime is { } end && end != DateTime.MinValue ? end - d.StartTime : TimeSpan.Zero;
                    return new
                    {
                        Item = new BO.ClosedDeliveryInList
                        {
                            DeliveryId = d.Id,
                            OrderId = d.OrderId,
                            OrderType = (BO.OrderType)(int)orderType,
                            FullAddress = fullAddress,
                            shipmentType = (BO.ShipmentType)(int)d.ShipmentType,
                            ActualDistance = d.ActualDistance,
                            TotalHandlingTime = handling,
                            DeliveryEndType = (BO.DeliveryEndType)(int)d.DeliveryEndType
                        },
                        // compute schedule status for sorting if needed
                        Schedule = order is null
                            ? DO.ScheduleStatus.OnTime
                            : ComputeScheduleStatus(order.CreatedAt + cfg.MaxDeliveryTime - (d.EndTime ?? now),
                                riskRange),
                        EndTime = d.EndTime ?? DateTime.MinValue,
                        Type = d.ShipmentType
                    };
                })
                .ToList();

            // sorting
            IEnumerable<dynamic> sorted = sortBy switch
            {
                BO.ClosedDeliverySortBy.DeliveryEndTime => list.OrderByDescending(x => x.EndTime),
                BO.ClosedDeliverySortBy.DeliveryType => list.OrderBy(x => x.Type),
                BO.ClosedDeliverySortBy.ScheduleStatus => list.OrderBy(x => x.Schedule),
                null => list.OrderBy(x => x.Schedule),
                _ => list.OrderBy(x => x.Schedule)
            };

            return sorted.Select(x => x.Item as BO.ClosedDeliveryInList);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("DAL: entity not found", ex);
        }
        catch (DO.DalNullException ex)
        {
            throw new BO.BlInvalidOperationException("DAL: null", ex);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("DAL: already exists", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlDalFileException("DAL: xml", ex);
        }
    }

    /// <summary>
    /// Return open orders available for selection by a courier.
    /// Implementation notes:
    /// - Use company coordinates from AdminManager.GetConfig().
    /// - Compute aerial distance (kilometers) between company and order.
    /// - Filter by courier.MaximumDistance when set.
    /// - If typeFilter provided filter by order type.
    /// - Default sort (when sortBy == null) is by ScheduleStatus.
    /// </summary>
    public static IEnumerable<BO.OpenOrderInList> GetOpenOrdersForCourier(int requesterId, int courierId,
        BO.OrderType? typeFilter, BO.OpenOrderSortBy? sortBy)
    {
        try
        {
            var cfg = AdminManager.GetConfig();
            var companyLat = cfg.Latitude ?? 0.0;
            var companyLon = cfg.Longitude ?? 0.0;
            var now = AdminManager.Now;
            var riskRange = cfg.RiskRange;

            // get courier DO to read maximum distance
            var courierDo = s_dal.Courier.Read(courierId);
            if (courierDo == null)
                throw new BO.BlDoesNotExistException($"Courier with id {courierId} not found");

            var courierMaxDistance = courierDo.MaximumDistance; // km, nullable

            // read open orders (DO.Order) and project to (OpenOrderInList + createdAt) for sorting
            var candidates =
                s_dal.Order.ReadAll(o => o.Status == DO.OrderStatus.Open)
                    .Where(o => o != null)
                    .Select(o =>
                    {
                        var aerialKm = ComputeAerialDistance(companyLat, companyLon, o.Latitude, o.Longitude);
                        var maxDeliveryDate = o.CreatedAt + cfg.MaxDeliveryTime;
                        var timeRemaining = maxDeliveryDate - now;
                        var scheduleStatus = ComputeScheduleStatus(timeRemaining, riskRange);
                        var actualDistance = Tools.CalculateDistance(companyLat, companyLon, o.Latitude, o.Longitude,
                            courierDo.ShipmentType);

                        var open = new BO.OpenOrderInList
                        {
                            OrderId = o.Id,
                            OrderType = (BO.OrderType)(int)o.Type,
                            FullAddress = o.Address,
                            AerialDistance = aerialKm,
                            ActualDistance = actualDistance,
                            EstimatedActualTime =
                                TimeSpan.FromHours(
                                    actualDistance / s_dal.Config.GetAverageSpeed(courierDo.ShipmentType)),
                            ScheduleStatus = scheduleStatus,
                            TimeRemaining = timeRemaining > TimeSpan.Zero ? timeRemaining : TimeSpan.Zero,
                            MaxDeliveryTime = maxDeliveryDate,
                            Volume = o.Volume,
                            CourierId = null,
                            Latitude = o.Latitude,
                            Longitude = o.Longitude
                        };

                        return (Open: open, CreatedAt: o.CreatedAt);
                    })
                    // filter by courier max distance
                    .Where(t => courierMaxDistance == null || t.Open.ActualDistance <= courierMaxDistance.Value)
                    // filter by type if requested
                    .Where(t => typeFilter == null || t.Open.OrderType == typeFilter.Value);

            // sorting
            IEnumerable<(BO.OpenOrderInList Open, DateTime CreatedAt)> sorted = sortBy switch
            {
                BO.OpenOrderSortBy.AerialDistance => candidates.OrderBy(t => t.Open.AerialDistance),
                BO.OpenOrderSortBy.OrderOpenTime => candidates.OrderByDescending(t => t.CreatedAt),
                BO.OpenOrderSortBy.ScheduleStatus => candidates.OrderBy(t => t.Open.ScheduleStatus),
                null => candidates.OrderBy(t => t.Open.ScheduleStatus), // default
                _ => candidates.OrderBy(t => t.Open.ScheduleStatus)
            };

            return sorted.Select(t => t.Open).ToList();
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("DAL: entity not found", ex);
        }
        catch (DO.DalNullException ex)
        {
            throw new BO.BlInvalidOperationException("DAL: null", ex);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("DAL: already exists", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlDalFileException("DAL: xml", ex);
        }
    }

    /// <summary>
    /// Computes aerial distance in kilometers between two WGS84 coordinates using the haversine formula.
    /// </summary>
    private static double ComputeAerialDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0; // Earth radius in kilometers
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);
        var a = Math.Pow(Math.Sin(dLat / 2), 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Pow(Math.Sin(dLon / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double DegreesToRadians(double deg)
    {
        return deg * (Math.PI / 180.0);
    }

    /// <summary>
    /// Compute schedule status from remaining time and risk range.
    /// </summary>
    internal static DO.ScheduleStatus ComputeScheduleStatus(TimeSpan timeRemaining, TimeSpan riskRange)
    {
        if (timeRemaining <= TimeSpan.Zero) return DO.ScheduleStatus.Late;
        if (timeRemaining <= riskRange) return DO.ScheduleStatus.AtRisk;
        return DO.ScheduleStatus.OnTime;
    }
}