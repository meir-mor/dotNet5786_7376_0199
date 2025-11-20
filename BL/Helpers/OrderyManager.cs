using DalApi;

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
                        .Select(d => new BO.DeliveryPerOrderInList
                        {
                            DeliveryId = d.Id,
                            CourierId = d.CourierId == 0 ? null : (int?)d.CourierId,
                            CourierName = s_dal.Courier.Read(d.CourierId) is DO.Courier c ? c.Name : string.Empty,
                            shipmentType = (BO.ShipmentType)Enum.Parse(typeof(BO.ShipmentType), d.ShipmentShipmentType.ToString()),
                            DeliveryStartTime = d.StartTime,
                            DeliveryEndType = d.DeliveryEndType is null ? null : (BO.DeliveryEndType?)Enum.Parse(typeof(BO.DeliveryEndType), d.DeliveryEndType.ToString()),
                            DeliveryEndTime = d.EndTime is DateTime time && time != DateTime.MinValue ? d.EndTime : null
                        })
                        .ToList(); // extension ToList
                    return bo;
                })
                .OrderByDescending(o => o.OrderOpenTime)
                .ToList();

            return filter == null ? orders : orders.Where(filter);
        }
        catch (Exception ex) { throw new BO.DalException("DAL: error while reading orders", ex); }
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
                group d by d.CourierId into g // grouping
                orderby g.Key
                select new
                {
                    CourierId = g.Key,
                    Items = g.Select(x => new BO.DeliveryPerOrderInList
                    {
                        DeliveryId = x.Id,
                        CourierId = x.CourierId == 0 ? null : (int?)x.CourierId,
                        CourierName = s_dal.Courier.Read(x.CourierId) is DO.Courier c ? c.Name : string.Empty,
                        shipmentType = (BO.ShipmentType)Enum.Parse(typeof(BO.ShipmentType), x.ShipmentShipmentType.ToString()),
                        DeliveryStartTime = x.StartTime,
                        DeliveryEndType = x.DeliveryEndType is null ? null : (BO.DeliveryEndType?)Enum.Parse(typeof(BO.DeliveryEndType), x.DeliveryEndType.ToString()),
                        DeliveryEndTime = x.EndTime is DateTime time && time != DateTime.MinValue ? x.EndTime : null
                    }).ToList()
                };

            // flatten grouped results to a single list of deliveries
            bo.Deliveries = deliveries.SelectMany(g => g.Items).ToList();

            return bo;
        }
        catch (Exception ex) { throw new BO.DalException($"DAL: error while reading order {id}", ex); }
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
                (DO.OrderType)Enum.Parse(typeof(DO.OrderType), bo.OrderType.ToString()),
                bo.Description,
                bo.FullAddress,
                bo.CustomerFullName,
                bo.CustomerPhone,
                false, // BO doesn't expose IsFragile; default false
                bo.OrderOpenTime,
                (DO.OrderStatus)Enum.Parse(typeof(DO.OrderStatus), bo.OrderStatus.ToString()),
                bo.Latitude,
                bo.Longitude
            );

            s_dal.Order.Create(doOrder);
        }
        catch (DO.DalAlreadyExistsException ex) { throw new BO.DalException("DAL: order already exists", ex); }
        catch (Exception ex) { throw new BO.DalException("DAL: unexpected error while creating order", ex); }
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
                (DO.OrderType)Enum.Parse(typeof(DO.OrderType), bo.OrderType.ToString()),
                bo.Description,
                bo.FullAddress,
                bo.CustomerFullName,
                bo.CustomerPhone,
                false,
                bo.OrderOpenTime,
                (DO.OrderStatus)Enum.Parse(typeof(DO.OrderStatus), bo.OrderStatus.ToString()),
                bo.Latitude,
                bo.Longitude
            );

            s_dal.Order.Update(doOrder);
        }
        catch (DO.DalDoesNotExistException ex) { throw new BO.DalException($"DAL: order {bo.Id} not found", ex); }
        catch (Exception ex) { throw new BO.DalException("DAL: unexpected error while updating order", ex); }
    }

    /// <summary>
    /// Delete order by id.
    /// </summary>
    internal static void DeleteOrder(int id)
    {
        try
        {
            var exists = s_dal.Order.ReadAll().Any(o => o.Id == id);
            if (!exists) throw new BO.DalException($"DAL: order {id} not found");

            s_dal.Order.Delete(id);
        }
        catch (DO.DalDoesNotExistException ex) { throw new BO.DalException($"DAL: order {id} does not exist", ex); }
        catch (Exception ex) { throw new BO.DalException("DAL: unexpected error while deleting order", ex); }
    }
    
    internal static void PeriodicOrdersUpdates(DateTime oldClock, DateTime newClock)
    {
    }

    private static BO.Order MapDoToBo(DO.Order o)
        => new()
        {
            Id = o.Id,
            OrderType = (BO.OrderType)Enum.Parse(typeof(BO.OrderType), o.Type.ToString()),
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
            OrderStatus = (BO.OrderStatus)Enum.Parse(typeof(BO.OrderStatus), o.Status.ToString()),
            ScheduleStatus = BO.ScheduleStatus.OnTime,
            TimeRemaining = TimeSpan.Zero,
            Deliveries = new List<BO.DeliveryPerOrderInList>()
        };
}
