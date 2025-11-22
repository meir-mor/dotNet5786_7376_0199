using BO;
using DalApi;
using DO;
using Order = BO.Order;

namespace Helpers;

internal static class OrderManager
{
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
                        .Select(d => new DeliveryPerOrderInList
                        {
                            DeliveryId = d.Id,
                            CourierId = d.CourierId == 0 ? null : (int?)d.CourierId,
                            CourierName = s_dal.Courier.Read(d.CourierId) is DO.Courier c ? c.Name : string.Empty,
                            shipmentType = (ShipmentType)Enum.Parse(typeof(ShipmentType), d.ShipmentType.ToString()),
                            DeliveryStartTime = d.StartTime,
                            DeliveryEndType = d.DeliveryEndType is null
                                ? null
                                : (DeliveryEndType?)Enum.Parse(typeof(DeliveryEndType), d.DeliveryEndType.ToString()),
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
                    Items = g.Select(x => new DeliveryPerOrderInList
                    {
                        DeliveryId = x.Id,
                        CourierId = x.CourierId == 0 ? null : (int?)x.CourierId,
                        CourierName = s_dal.Courier.Read(x.CourierId) is DO.Courier c ? c.Name : string.Empty,
                        shipmentType = (ShipmentType)Enum.Parse(typeof(ShipmentType), x.ShipmentType.ToString()),
                        DeliveryStartTime = x.StartTime,
                        DeliveryEndType = x.DeliveryEndType is null
                            ? null
                            : (DeliveryEndType?)Enum.Parse(typeof(DeliveryEndType), x.DeliveryEndType.ToString()),
                        DeliveryEndTime = x.EndTime is DateTime time && time != DateTime.MinValue ? x.EndTime : null
                    }).ToList()
                };

            // flatten grouped results to a single list of deliveries
            bo.Deliveries = deliveries.SelectMany(g => g.Items).ToList();

            return bo;
        }
        catch (BlException ex)
        {
            throw new BO.BlUnexpectedException($"DAL: error while reading order {id}", ex);
        }
    }

    /// <summary>
    /// Create order (maps BO -> DO).
    /// </summary>
    internal static void CreateOrder(BO.Order bo)
    {
        try
        {
            var doOrder = new DO.Order(
                0,
                bo.OrderType,
                bo.Description,
                bo.FullAddress,
                bo.CustomerFullName,
                bo.CustomerPhone,
                false, // BO doesn't expose IsFragile; default false
                bo.OrderOpenTime,
                bo.OrderStatus,
                bo.Latitude,
                bo.Longitude
            );

            s_dal.Order.Create(doOrder);
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
            var doOrder = new DO.Order(
                bo.Id,
                bo.OrderType,
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
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"DAL: order {bo.Id} not found", ex);
        }
    }

    /// <summary>
    /// Delete order by id.
    /// </summary>
    internal static void DeleteOrder(int id)
    {
        try
        {
            var exists = s_dal.Order.ReadAll().Any(o => o.Id == id);
            if (!exists) throw new BlException($"DAL: order {id} not found");

            s_dal.Order.Delete(id);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"DAL: order {id} does not exist", ex);
        }
    }

    internal static void PeriodicOrdersUpdates(DateTime oldClock, DateTime newClock)
    {
    }

    private static BO.Order MapDoToBo(DO.Order o)
    {
        return new Order
        {
            Id = o.Id,
            OrderType = (OrderType)Enum.Parse(typeof(OrderType), o.Type.ToString()),
            Description = o.Description,
            FullAddress = o.Address,
            Latitude = o.Latitude,
            Longitude = o.Longitude,
            AerialDistance = 0, // computed elsewhere if needed
            CustomerFullName = o.CustomerName,
            CustomerPhone = o.CustomerPhone,
            Volume = null,
            OrderOpenTime = o.CreatedAt,
            ExpectedDeliveryTime = DateTime.MinValue,
            MaxDeliveryTime = TimeSpan.Zero,
            OrderStatus = (OrderStatus)Enum.Parse(typeof(OrderStatus), o.Status.ToString()),
            ScheduleStatus = ScheduleStatus.OnTime,
            TimeRemaining = TimeSpan.Zero,
            Deliveries = new List<DeliveryPerOrderInList>()
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
                var orderStatus = (OrderStatus)Enum.Parse(typeof(OrderStatus), o.Status.ToString());
                return new { OrderStatus = orderStatus, Schedule = schedule };
            });

            // group by both fields and count
            var groups = pairs
                .GroupBy(p => new { p.OrderStatus, p.Schedule })
                .Select(g => new { g.Key.OrderStatus, g.Key.Schedule, Count = g.Count() })
                .ToList();

            var orderStatusCount = Enum.GetValues(typeof(OrderStatus)).Length;
            var scheduleCount = Enum.GetValues(typeof(ScheduleStatus)).Length;

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
            var order = s_dal.Order.Read(orderId) ?? throw new BlException($"Order {orderId} not found");

            var status = order.Status;
            if (status == OrderStatus.Open)
            {
                // create dummy delivery with start==end == now, cancelled, courier 0
                var dummy = new Delivery(
                    0,
                    orderId,
                    0,
                    ShipmentType.Motorcycle,
                    now,
                    null,
                    DeliveryEndType.Cancelled,
                    now
                );
                s_dal.Delivery.Create(dummy);
            }
            else if (status == OrderStatus.InProgress)
            {
                // find active delivery for this order (EndTime null)
                var delivery = s_dal.Delivery
                                   .ReadAll(d =>
                                       d.OrderId == orderId && (d.EndTime == null || d.DeliveryEndType == null))
                                   .FirstOrDefault()
                               ?? throw new BlException($"No active delivery found for order {orderId}");
                var updated = delivery with { EndTime = now, DeliveryEndType = DeliveryEndType.Cancelled };
                s_dal.Delivery.Update(updated);
            }
            else
            {
                throw new BO.BlInvalidOperationException($"Order {orderId} cannot be cancelled (status {status})");
            }
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

            // update delivery end and type
            var updated = delivery with { EndTime = now, DeliveryEndType = DeliveryEndType.Supplied };
            s_dal.Delivery.Update(updated);
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
            var order = s_dal.Order.Read(orderId) ?? throw new BlException($"Order {orderId} not found");

            if (order.Status != OrderStatus.Open)
                throw new BlException("Order is not open for assignment");

            // ensure no active delivery exists for this order
            var active = s_dal.Delivery
                .ReadAll(d => d.OrderId == orderId && (d.EndTime == null || d.DeliveryEndType == null))
                .FirstOrDefault();
            if (active != null)
                throw new BlException("Order already has an active delivery");

            // create delivery (use default ShipmentType - can be improved)
            var delivery = new Delivery(
                0,
                orderId,
                courierId,
                ShipmentType.Motorcycle,
                now,
                null,
                null,
                null
            );

            s_dal.Delivery.Create(delivery);

            // Optionally update order status to InProgress
            var updatedOrder = order with { Status = OrderStatus.InProgress };
            s_dal.Order.Update(updatedOrder);
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

    public static IEnumerable<ClosedDeliveryInList> GetClosedDeliveriesForCourier(int requesterId, int courierId,
        ShipmentType? typeFilter, ClosedDeliverySortBy? sortBy)
    {
        try
        {
            var cfg = AdminManager.GetConfig();
            var now = AdminManager.Now;
            var riskRange = cfg.RiskRange;

            var query = s_dal.Delivery.ReadAll(d => d.CourierId == courierId && d.DeliveryEndType != null);

            var list = query
                .Where(d => typeFilter == null || d.ShipmentType == typeFilter.Value)
                .Select(d =>
                {
                    var order = s_dal.Order.Read(d.OrderId);
                    var orderType = order is null
                        ? OrderType.General
                        : (OrderType)Enum.Parse(typeof(OrderType), order.Type.ToString());
                    var fullAddress = order is null ? string.Empty : order.Address;
                    var handling = d.EndTime is { } end && end != DateTime.MinValue ? end - d.StartTime : TimeSpan.Zero;
                    return new
                    {
                        Item = new ClosedDeliveryInList
                        {
                            DeliveryId = d.Id,
                            OrderId = d.OrderId,
                            OrderType = orderType,
                            FullAddress = fullAddress,
                            shipmentType = d.ShipmentType,
                            ActualDistance = d.ActualDistance,
                            TotalHandlingTime = handling,
                            DeliveryEndType = d.DeliveryEndType
                        },
                        // compute schedule status for sorting if needed
                        Schedule = order is null
                            ? ScheduleStatus.OnTime
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
                ClosedDeliverySortBy.DeliveryEndTime => list.OrderByDescending(x => x.EndTime),
                ClosedDeliverySortBy.DeliveryType => list.OrderBy(x => x.Type),
                ClosedDeliverySortBy.ScheduleStatus => list.OrderBy(x => x.Schedule),
                null => list.OrderBy(x => x.Schedule),
                _ => list.OrderBy(x => x.Schedule)
            };

            return sorted.Select(x => x.Item as ClosedDeliveryInList);
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
    public static IEnumerable<OpenOrderInList> GetOpenOrdersForCourier(int requesterId, int courierId,
        OrderType? typeFilter, OpenOrderSortBy? sortBy)
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
                s_dal.Order.ReadAll(o => o.Status == OrderStatus.Open)
                    .Where(o => o != null)
                    .Select(o =>
                    {
                        var aerialKm = ComputeAerialDistance(companyLat, companyLon, o.Latitude, o.Longitude);
                        var maxDeliveryDate = o.CreatedAt + cfg.MaxDeliveryTime;
                        var timeRemaining = maxDeliveryDate - now;
                        var scheduleStatus = ComputeScheduleStatus(timeRemaining, riskRange);

                        var open = new OpenOrderInList
                        {
                            OrderId = o.Id,
                            OrderType = (OrderType)Enum.Parse(typeof(OrderType), o.Type.ToString()),
                            FullAddress = o.Address,
                            AerialDistance = aerialKm,
                            ActualDistance = null,
                            EstimatedActualTime = null,
                            ScheduleStatus = scheduleStatus,
                            TimeRemaining = timeRemaining > TimeSpan.Zero ? timeRemaining : TimeSpan.Zero,
                            MaxDeliveryTime = maxDeliveryDate,
                            Volume = null,
                            CourierId = null
                        };

                        return (Open: open, CreatedAt: o.CreatedAt);
                    })
                    // filter by courier max distance
                    .Where(t => courierMaxDistance == null || t.Open.AerialDistance <= courierMaxDistance.Value)
                    // filter by type if requested
                    .Where(t => typeFilter == null || t.Open.OrderType == typeFilter.Value)
                    .ToList();

            // sorting
            IEnumerable<(OpenOrderInList Open, DateTime CreatedAt)> sorted = sortBy switch
            {
                OpenOrderSortBy.AerialDistance => candidates.OrderBy(t => t.Open.AerialDistance),
                OpenOrderSortBy.OrderOpenTime => candidates.OrderByDescending(t => t.CreatedAt),
                OpenOrderSortBy.ScheduleStatus => candidates.OrderBy(t => t.Open.ScheduleStatus),
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
    internal static ScheduleStatus ComputeScheduleStatus(TimeSpan timeRemaining, TimeSpan riskRange)
    {
        if (timeRemaining <= TimeSpan.Zero) return ScheduleStatus.Late;
        if (timeRemaining <= riskRange) return ScheduleStatus.AtRisk;
        return ScheduleStatus.OnTime;
    }
}