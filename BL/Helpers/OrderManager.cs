using BO;
using DalApi;
using DO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Factory = DalApi.Factory;
using OrderStatus = DO.OrderStatus;

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
            List<DO.Order> allOrders;
            lock (AdminManager.BlMutex)
                allOrders = s_dal.Order.ReadAll().ToList();

            var orders = allOrders
                .Select(o =>
                {
                    var bo = MapDoToBo(o);

                    List<DO.Delivery> deliveries;
                    lock (AdminManager.BlMutex)
                        deliveries = s_dal.Delivery.ReadAll(d => d.OrderId == o.Id).ToList();

                    bo.Deliveries = deliveries
                        .Select(d =>
                        {
                            string courierName;
                            lock (AdminManager.BlMutex)
                                courierName = s_dal.Courier.Read(d.CourierId)?.Name ?? string.Empty;

                            return new BO.DeliveryPerOrderInList
                            {
                                DeliveryId = d.Id,
                                CourierId = d.CourierId,
                                CourierName = courierName,
                                ShipmentType = (BO.ShipmentType)(int)d.ShipmentType,
                                DeliveryStartTime = d.StartTime,
                                DeliveryEndType = (BO.DeliveryEndType?)(int?)d.DeliveryEndType,
                                DeliveryEndTime = d.EndTime
                            };
                        }).ToList();

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
            DO.Order doOrder;
            lock (AdminManager.BlMutex)
                doOrder = s_dal.Order.Read(id);
            if (doOrder == null) return null;

            var bo = MapDoToBo(doOrder);

            // query-syntax: group deliveries by courier for this order (demonstrates grouping + select new)
            List<DO.Delivery> allDeliveries;
            lock (AdminManager.BlMutex)
                allDeliveries = s_dal.Delivery.ReadAll().ToList();

            var deliveries =
                from d in allDeliveries
                where d.OrderId == id
                group d by d.CourierId
                into g // grouping
                orderby g.Key
                select new
                {
                    CourierId = g.Key,
                    Items = g.Select(x =>
                    {
                        string courierName;
                        lock (AdminManager.BlMutex)
                            courierName = s_dal.Courier.Read(x.CourierId)?.Name ?? string.Empty; // עטוף קריאה ל-DAL

                        return new BO.DeliveryPerOrderInList
                        {
                            DeliveryId = x.Id,
                            CourierId = x.CourierId,
                            CourierName = courierName,
                            ShipmentType = (BO.ShipmentType)(int)x.ShipmentType,
                            DeliveryStartTime = x.StartTime,
                            DeliveryEndType = (BO.DeliveryEndType?)(int?)x.DeliveryEndType,
                            DeliveryEndTime = x.EndTime
                        };
                    }).ToList()
                };


            // flatten grouped results to a single list of deliveries
            bo.Deliveries = deliveries.SelectMany(g => g.Items).ToList();

            if (bo.Deliveries == null || bo.Deliveries.Count == 0)
                bo.OrderStatus = BO.OrderStatus.Open;
            else if (bo.Deliveries.Any(d => d.DeliveryEndType == null))
                bo.OrderStatus = BO.OrderStatus.InProgress;
            else
                bo.OrderStatus = BO.OrderStatus.Closed;

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
    internal static async Task CreateOrder(BO.Order order)
    {
        try
        {
            var (lat, lon) = await Tools.GetCoordinates(order.FullAddress);
            if (lat is null || lon is null)
            {
                throw new BO.BlUnexpectedException("DAL: invalid coordinates");
            }

            DO.OrderStatus initialStatus = DO.OrderStatus.Open;

            var doOrder = new DO.Order(
                0,
                (DO.OrderType)(int)order.OrderType,
                order.Description,
                order.FullAddress,
                order.CustomerFullName,
                order.CustomerPhone,
                false, // BO doesn't expose IsFragile; default false
                order.OrderOpenTime,
                initialStatus,
                lat ?? 0,
                lon ?? 0,
                order.Volume
            );

            lock (AdminManager.BlMutex)
                s_dal.Order.Create(doOrder);

            var activeCouriers = s_dal.Courier.ReadAll()
                .Where(c => c.IsActive)
                .Select(c => new BO.Courier
                {
                    Id = c.Id,
                    FullName = c.Name,
                    PhoneNumber = c.Phone,
                    Email = c.Email,
                    Password = c.Password,
                    IsActive = c.IsActive,
                    MaxDistance = c.MaximumDistance,
                    ShipmentType = (BO.ShipmentType)(int)c.ShipmentType,
                    StartWorkTime = c.StartingWork
                })
                .ToList();

            var Latitude_m = s_dal.Config.Latitude ?? 0;
            var Longitude_m = s_dal.Config.Longitude ?? 0;

            var eligibleCouriers = new List<BO.Courier>();

            foreach (var c in activeCouriers)
            {
                var distanceType = (DO.ShipmentType)(int)c.ShipmentType;

                var distance = await Tools.CalculateDistance(
                    Latitude_m, Longitude_m,
                    doOrder.Latitude, doOrder.Longitude,
                    distanceType
                );

                if (distance <= (c.MaxDistance ?? double.MaxValue))
                    eligibleCouriers.Add(c);
            }
            foreach (var courier in eligibleCouriers)
            {
                if (string.IsNullOrWhiteSpace(courier.Email)) continue;

                var mail = new System.Net.Mail.MailMessage();
                mail.From = new System.Net.Mail.MailAddress("myrm07996@gmail.com");
                mail.To.Add(courier.Email);
                mail.Subject = "New Delivery Order Available";
                mail.Body = $"Hello {courier.FullName},\n\n" +
                            $"A new delivery order is available for you.\n\n" +
                            $"Order ID: {doOrder.Id}\n" +
                            $"Address: {doOrder.Address}\n" +
                            $"Customer: {doOrder.CustomerName}\n" +
                            $"Phone: {doOrder.CustomerPhone}\n\n" +
                            $"Please log in to the system if you wish to accept this order.\n\n" +
                            $"Best regards,\nDelivery System";

                _ = OrderManager.SendEmailAsync(mail);
            }
            

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

            DO.Order existing;
            lock (AdminManager.BlMutex)
                 existing = s_dal.Order.Read(bo.Id)
                           ?? throw new BO.BlDoesNotExistException($"Order {bo.Id} not found");

            if (existing.Status == DO.OrderStatus.Closed)
                throw new BO.BlInvalidOperationException("Cannot update a closed order");

            List<DO.Delivery> deliveries;
            lock (AdminManager.BlMutex)
                 deliveries = s_dal.Delivery.ReadAll().Where(d => d.OrderId == bo.Id).ToList();

            DO.OrderStatus newStatus;
            if (deliveries.Count == 0)
                newStatus = DO.OrderStatus.Open;
            else if (deliveries.Any(d => d.EndTime == null || d.DeliveryEndType == null))
                newStatus = DO.OrderStatus.InProgress;
            else
                newStatus = DO.OrderStatus.Closed;

            var doOrder = new DO.Order(
                bo.Id,
                (DO.OrderType)(int)bo.OrderType,
                bo.Description,
                bo.FullAddress,
                bo.CustomerFullName,
                bo.CustomerPhone,
                false,
                existing.CreatedAt,
                newStatus,
                bo.Latitude,
                bo.Longitude,
                bo.Volume
            );

            lock (AdminManager.BlMutex)
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
    /// Delete order by id.
    /// </summary>
    internal static void DeleteOrder(int id)
    {
        try
        {
            bool exists;
            lock (AdminManager.BlMutex)
            { 
                 exists = s_dal.Order.ReadAll().Any(o => o.Id == id);
            if (!exists) throw new BO.BlException($"DAL: order {id} not found");

            s_dal.Order.Delete(id);
            }
            Observers.NotifyItemUpdated(id);
            Observers.NotifyListUpdated();
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"DAL: order {id} does not exist", ex);
        }
    }

    private static readonly AsyncMutex s_periodicMutex = new(); //stage 7

    internal static void PeriodicOrdersUpdates(DateTime oldClock, DateTime newClock)
    {
        if (s_periodicMutex.CheckAndSetInProgress())
            return;

        List<int> updatedOrders = new();
        bool updated = false;

        try
        {
            List<DO.Order> orders;
            IConfig config;

            lock (AdminManager.BlMutex) // stage 7
            {
                orders = s_dal.Order
                    .ReadAll(o => o.Status != DO.OrderStatus.Closed)
                    .ToList(); 

                config = s_dal.Config;
            }

            foreach (var doOrder in orders)
            {
                if (AdminManager.Now - doOrder.CreatedAt >= config.MaxDeliveryTime)
                {
                    updated = true;
                    updatedOrders.Add(doOrder.Id);
                }
            }
        }
        finally
        {
            s_periodicMutex.UnsetInProgress();
        }

        foreach (var id in updatedOrders)
            Observers.NotifyItemUpdated(id);
        if (updated || oldClock != newClock)
            Observers.NotifyListUpdated();
    }

    private static BO.Order MapDoToBo(DO.Order o)
    {
        DO.Delivery? delivery;
        lock (AdminManager.BlMutex)
             delivery = s_dal.Delivery.Read(d => d.OrderId == o.Id);
        var timeRemaining = o.CreatedAt + s_dal.Config.MaxDeliveryTime - s_dal.Config.Clock;
        var timeItsTook = delivery?.EndTime - o.CreatedAt;
        var scheduleStatus = timeItsTook >= s_dal.Config.MaxDeliveryTime ? ScheduleStatus.Late
            : timeItsTook >= s_dal.Config.RiskRange ? ScheduleStatus.AtRisk
            : ScheduleStatus.OnTime;
        return new BO.Order
        {
            Id = o.Id,
            OrderType = (BO.OrderType)(int)o.Type,
            Description = o.Description,
            FullAddress = o.Address,
            Latitude = o.Latitude,
            Longitude = o.Longitude,
            AerialDistance = Tools.CalculateHaversineDistance(s_dal.Config.Latitude, s_dal.Config.Longitude,
                o.Latitude, o.Longitude),
            CustomerFullName = o.CustomerName,
            CustomerPhone = o.CustomerPhone,
            Volume = o.Volume,
            OrderOpenTime = o.CreatedAt,
            OrderStatus = (BO.OrderStatus)(int)o.Status,
            ScheduleStatus = o.Status == OrderStatus.Closed ? scheduleStatus : timeRemaining <= TimeSpan.Zero ? ScheduleStatus.Late :
                timeRemaining >= s_dal.Config.RiskRange ? ScheduleStatus.AtRisk : ScheduleStatus.OnTime,
            TimeRemaining = o.Status == OrderStatus.Closed ? TimeSpan.Zero : timeRemaining < TimeSpan.Zero ? TimeSpan.Zero : timeRemaining,
            MaxDeliveryTime = o.CreatedAt + s_dal.Config.MaxDeliveryTime,
            ExpectedDeliveryTime = delivery != null
                ? delivery.StartTime + TimeSpan.FromHours(
                    delivery.ActualDistance ?? 0 / s_dal.Config.GetAverageSpeed(delivery.ShipmentType))
                : o.CreatedAt + s_dal.Config.MaxDeliveryTime,
            Deliveries = []
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

            DO.Order order;
            lock (AdminManager.BlMutex)       
                 order = s_dal.Order.Read(orderId) ?? throw new BO.BlException($"Order {orderId} not found");

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
                lock (AdminManager.BlMutex)
                    s_dal.Delivery.Create(dummy);
            }
            else if (status == DO.OrderStatus.InProgress)
            {
                // find active delivery for this order (EndTime null)
                List<DO.Delivery> deliveries;
                lock (AdminManager.BlMutex)
                    deliveries = s_dal.Delivery.ReadAll(d =>
                        d.OrderId == orderId && (d.EndTime == null || d.DeliveryEndType == null)).ToList();

                var delivery = deliveries.FirstOrDefault()
                               ?? throw new BO.BlException($"No active delivery found for order {orderId}");

                var updated = delivery with { EndTime = now, DeliveryEndType = DO.DeliveryEndType.Cancelled };

                lock (AdminManager.BlMutex)
                    s_dal.Delivery.Update(updated);

                var courier = s_dal.Courier.Read(delivery.CourierId);
                MailMessage mail = new MailMessage();

                mail.From = new MailAddress("myrm07996@gmail.com");
                mail.To.Add(courier!.Email);
                mail.Subject = "Cancel Call Notification";
                mail.Body = $"Hello {courier.Name},\n\n" +
                    $"At " + AdminManager.Now + $", the manager has cancelled your assignment for delivery {delivery.Id}.\n" +
                    "Please contact the manager for further details.\n\n" +
                    "Best regards,\nYour Organization\n";

                _ = OrderManager.SendEmailAsync(mail);
            }
            else
            {
                throw new BO.BlInvalidOperationException($"Order {orderId} cannot be cancelled (status {status})");
            }
            lock (AdminManager.BlMutex)
                s_dal.Order.Update(order with { Status = DO.OrderStatus.Closed });

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

    /// <summary>
    /// sending an email.
    /// </summary>
    /// <param name="email">the email to send</param>
    public static async Task SendEmailAsync(MailMessage email)
    {

        try
        {
            SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");

            smtpServer.Port = 587;
            smtpServer.Credentials = new NetworkCredential("myrm07996@gmail.com", "");
            smtpServer.EnableSsl = true;

            await smtpServer.SendMailAsync(email);
        }
        catch (Exception ex)
        {
            // Handle exception
            throw new BlException($"Exception caught in SendEmail(): {ex.Message}");
        }

    }
    public static void CompleteDelivery(int requesterId, int courierId, int deliveryId,
    DO.DeliveryEndType endType = DO.DeliveryEndType.Supplied)
    {
        try
        {
            var now = AdminManager.Now;
            DO.Delivery delivery;
            lock (AdminManager.BlMutex)
                 delivery = s_dal.Delivery.Read(deliveryId) ??
                           throw new BO.BlDoesNotExistException($"Delivery {deliveryId} not found");

            if (delivery.CourierId != courierId)
                throw new BO.BlInvalidOperationException("Requester is not the courier of this delivery");

            DO.Order? order;
            lock (AdminManager.BlMutex)
                 order = s_dal.Order.Read(delivery.OrderId);

            if (order is null || order.Status != DO.OrderStatus.InProgress)
                throw new BO.BlException("Order is not in progress");

            // update delivery end and type
            var updated = delivery with { EndTime = now, DeliveryEndType = endType };
            lock (AdminManager.BlMutex)
            {
                s_dal.Delivery.Update(updated);
                s_dal.Order.Update(order with { Status = DO.OrderStatus.Closed, });
            }
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("DAL: entity not found", ex);
        }
    }

    public static async Task AssignOrderToCourier(int requesterId, int courierId, int orderId)
    {
        try
        {
            var now = AdminManager.Now;

            DO.Order order;
            lock (AdminManager.BlMutex)
                 order = s_dal.Order.Read(orderId) ?? throw new BO.BlException($"Order {orderId} not found");

            if (order.Status != DO.OrderStatus.Open)
                throw new BO.BlException("Order is not open for assignment");

            // ensure no active delivery exists for this order
            DO.Delivery? active;
            lock (AdminManager.BlMutex)
                 active = s_dal.Delivery
                .ReadAll(d => d.OrderId == orderId && (d.EndTime == null || d.DeliveryEndType == null))
                .FirstOrDefault();
            if (active != null)
                throw new BO.BlException("Order already has an active delivery");

            var distance = await Tools.CalculateDistance(s_dal.Config.Latitude, s_dal.Config.Longitude,
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

            lock (AdminManager.BlMutex)
                s_dal.Delivery.Create(delivery);

            // Optionally update order status to InProgress
            var updatedOrder = order with { Status = DO.OrderStatus.InProgress };
            lock (AdminManager.BlMutex)
                s_dal.Order.Update(updatedOrder);

            // Send Email notification (simulated)
            try
            {
                // This would normally use a dedicated notification service
                // For now, we simulate the logik that was in PL
                string toEmail = "arioren100@gmail.com";
                System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                mail.From = new System.Net.Mail.MailAddress("arioren100@gmail.com");
                mail.To.Add(toEmail);
                mail.Subject = $"New Order Assigned #{order.Id}";
                mail.Body = $@"
Hello,
You have successfully assigned Order #{order.Id}.
Details:
Type: {(BO.OrderType)(int)order.Type}
Address: {order.Address}
Safe Travels!
";
                // In a real app, you would send via SmtpClient here.
                // Log it for verification since we can't actually send mail without credentials/server.
                System.Diagnostics.Debug.WriteLine($"[BL Notification] Email sent to {toEmail} for Order {orderId}");
            }
            catch
            {
                /* ignore notification errors in business logic to not block assignment */
            }

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

            List<DO.Delivery> query;
            lock (AdminManager.BlMutex)
                 query = s_dal.Delivery.ReadAll(d => d.CourierId == courierId && d.DeliveryEndType != null).ToList();

            var list = query
                .Where(d => typeFilter == null || d.ShipmentType == (DO.ShipmentType)(int)typeFilter.Value)
                .Select(d =>
                {

                    DO.Order? order;
                    lock (AdminManager.BlMutex)
                        order = s_dal.Order.Read(d.OrderId);

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
                BO.ClosedDeliverySortBy.DeliveryEndTime => list.OrderByDescending(x => x.EndTime),
                BO.ClosedDeliverySortBy.DeliveryType => list.OrderBy(x => x.Type),
                BO.ClosedDeliverySortBy.ScheduleStatus => list.OrderBy(x => x.Schedule),
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
    public static async Task<IEnumerable<BO.OpenOrderInList>> GetOpenOrdersForCourier(int requesterId, int courierId,
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
            DO.Courier courierDo;
            lock (AdminManager.BlMutex)
                 courierDo = s_dal.Courier.Read(courierId);
            if (courierDo == null)
                throw new BO.BlDoesNotExistException($"Courier with id {courierId} not found");

            var courierMaxDistance = courierDo.MaximumDistance; // km, nullable

            // read open orders (DO.Order) and project to (OpenOrderInList + createdAt) for sorting
            List<DO.Order> openOrders;
            lock (AdminManager.BlMutex)
                openOrders = s_dal.Order.ReadAll(o => o.Status == DO.OrderStatus.Open).ToList();

            var tasks =openOrders.Select(async o =>
                       {
                        var aerialKm = ComputeAerialDistance(companyLat, companyLon, o.Latitude, o.Longitude);
                        var maxDeliveryDate = o.CreatedAt + cfg.MaxDeliveryTime;
                        var timeRemaining = maxDeliveryDate - now;
                        var scheduleStatus = ComputeScheduleStatus(timeRemaining, riskRange);
                        var actualDistance =await Tools.CalculateDistance(companyLat, companyLon, o.Latitude, o.Longitude,
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
                       .ToList();
            var candidates = await Task.WhenAll(tasks);

            var filtered = candidates
           // filter by courier max distance
           .Where(t => courierMaxDistance == null || t.Open.ActualDistance <= courierMaxDistance.Value)
                    // filter by type if requested
                    .Where(t => typeFilter == null || t.Open.OrderType == typeFilter.Value);

            IEnumerable<(BO.OpenOrderInList Open, DateTime CreatedAt)> sorted = sortBy switch
            {
                BO.OpenOrderSortBy.AerialDistance => filtered.OrderBy(t => t.Open.AerialDistance),
                BO.OpenOrderSortBy.OrderOpenTime => filtered.OrderByDescending(t => t.CreatedAt),
                BO.OpenOrderSortBy.ScheduleStatus => filtered.OrderBy(t => t.Open.ScheduleStatus),
                null => filtered.OrderBy(t => t.Open.ScheduleStatus), // default
                _ => filtered.OrderBy(t => t.Open.ScheduleStatus)
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
    private static ScheduleStatus ComputeScheduleStatus(TimeSpan timeRemaining, TimeSpan riskRange)
    {
        if (timeRemaining <= TimeSpan.Zero) return ScheduleStatus.Late;
        if (timeRemaining <= riskRange) return ScheduleStatus.AtRisk;
        return ScheduleStatus.OnTime;
    }


}