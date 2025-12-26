using BO;
using DalApi;
using System;
using DeliveryEndType = DO.DeliveryEndType;
using Factory = DalApi.Factory;
using OrderStatus = DO.OrderStatus;

namespace Helpers;

internal static class CourierManager
{
    internal static ObserverManager Observers = new(); //stage 5 

    // DAL access (private field as requested)
    private static IDal s_dal = Factory.Get; //stage 4


    /// <summary>
    /// Read a single courier by id and map to BO.Courier.
    /// Uses extension method FirstOrDefault.
    /// </summary>
    internal static BO.Courier? GetCourierById(int id)
    {
        try
        {
            var courier = s_dal.Courier.Read(id);

            var orders = s_dal.Order.ReadAll(o =>
                s_dal.Delivery.ReadAll(d =>
                    d.CourierId == id).Select(d => d.OrderId).Contains(o.Id)).ToArray();

            var ordersIds = orders.Select(o => o.Id).ToArray();
            var deliveries = s_dal.Delivery.ReadAll(d => ordersIds.Contains(d.OrderId)).ToArray();

            var totalDeliveredOnTime = deliveries.Where(d =>d.DeliveryEndType == DeliveryEndType.Supplied && d.EndTime - d.StartTime <= s_dal.Config.MaxDeliveryTime)?.ToArray().Length;
            var totalDeliveredLate = deliveries.Where(d =>d.DeliveryEndType == DeliveryEndType.Supplied && d.EndTime - d.StartTime > s_dal.Config.MaxDeliveryTime)?.ToArray().Length;
            
            var order = orders.FirstOrDefault(o => o.Status == OrderStatus.InProgress);
            var delivery = deliveries.FirstOrDefault(d => d.OrderId == order?.Id);
            if (courier == null) return null;

            // map DO.Courier -> BO.Courier
            var bo = new BO.Courier
            {
                Id = courier.Id,
                FullName = courier.Name,
                PhoneNumber = courier.Phone,
                Email = courier.Email,
                Password = courier.Password,
                IsActive = courier.IsActive,
                MaxDistance = courier.MaximumDistance ?? null,
                ShipmentType = (BO.ShipmentType)(int)courier.ShipmentType,
                StartWorkTime = courier.StartingWork,
                TotalDeliveredOnTime = totalDeliveredOnTime ?? 0,
                TotalDeliveredLate = totalDeliveredLate ?? 0,
                OrderInProgress = CreateOrderInProgress(order, delivery),
            };

            return bo;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"DAL: courier with id {id} does not exist", ex);
        }
    }

    public static OrderInProgress? CreateOrderInProgress(DO.Order? order, DO.Delivery? delivery)
    {
        if (order == null || delivery == null) return null;
        return new BO.OrderInProgress
        {
            OrderId = order.Id,
            ActualDistance = delivery.ActualDistance ?? 0,
            DeliveryId = delivery.Id,
            CustomerFullName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            DeliveryStartTime = delivery.StartTime,
            OrderType = (BO.OrderType)(int)order.Type,
            OrderStatus = (BO.OrderStatus)order.Status,
            Description = order.Description,
            AerialDistance = Tools.CalculateHaversineDistance(s_dal.Config.Latitude ?? 0, s_dal.Config.Longitude ?? 0, order.Latitude, order.Longitude),
            OrderOpenTime = order.CreatedAt,
            ExpectedDeliveryTime = order.CreatedAt + TimeSpan.FromHours(delivery.ActualDistance ?? 0 / s_dal.Config.GetAverageSpeed(delivery.ShipmentType)
            ),
            MaxDeliveryTime = order.CreatedAt + s_dal.Config.MaxDeliveryTime,
            ScheduleStatus = DO.ScheduleStatus.OnTime,
            TimeRemaining = order.CreatedAt + s_dal.Config.MaxDeliveryTime - s_dal.Config.Clock,
            FullAddress = order.Address,
        };
    }
    /// <summary>
    /// Return all couriers converted to BO.CourierInList.
    /// Uses LINQ extension methods (Where, Select, OrderBy, ToList).
    /// Wrapped in try/catch to translate DO exceptions into BO.DalException.
    /// </summary>
    public static IEnumerable<BO.CourierInList> GetAllCouriersInList(
        Func<BO.CourierInList, bool>? customFilter = null, BO.ShipmentType? filter = null)
    {
        try
        {
            var deliveries = s_dal.Delivery.ReadAll();
            var config = s_dal.Config;

            var result = s_dal.Courier.ReadAll()
                .Where(c => c != null)
                .Select(c => new BO.CourierInList
                {
                    Id = c.Id,
                    FullName = c.Name,
                    IsActive = c.IsActive,
                    ShipmentType = (BO.ShipmentType)(int)c.ShipmentType,
                    StartWorkTime = c.StartingWork,
                    TotalDeliveredOnTime = deliveries.Count(d =>
                        d.CourierId == c.Id &&
                        d.DeliveryEndType == DO.DeliveryEndType.Supplied &&
                        d.EndTime - d.StartTime <= config.MaxDeliveryTime),
                    TotalDeliveredLate = deliveries.Count(d =>
                        d.CourierId == c.Id &&
                        d.DeliveryEndType == DO.DeliveryEndType.Supplied &&
                        d.EndTime - d.StartTime > config.MaxDeliveryTime),
                    CurrentDeliveryId = deliveries.Count(d =>
                        d.CourierId == c.Id && d.EndTime == null)
                })
                .ToList();

            if (filter.HasValue)
                result = result.Where(c => c.ShipmentType == filter.Value).ToList();

            if (customFilter != null)
                result = result.Where(customFilter).ToList();

            return result;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("DAL: requested courier does not exist", ex);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("DAL: entity already exists", ex);
        }
        catch (DO.DalNullException ex)
        {
            throw new BO.BlInvalidOperationException("DAL: null encountered", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlDalFileException("DAL: XML file load/create error", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: unexpected error while reading couriers", ex);
        }
    }

    /// <summary>
    /// Create a new courier in DAL.
    /// Receives BO.Courier and explicit DO.ShipmentType (BO.Courier doesn't contain shipment type in current model).
    /// Maps BO -> DO and calls DAL Create.
    /// </summary>
    internal static void CreateCourier(BO.Courier boCourier)
    {
        try
        {
            var doCourier = new DO.Courier(
                boCourier.Id,
                boCourier.FullName,
                boCourier.PhoneNumber,
                boCourier.Email,
                boCourier.Password ?? string.Empty,
                boCourier.IsActive,
                boCourier.MaxDistance,
                (DO.ShipmentType)(int)boCourier.ShipmentType,
                boCourier.StartWorkTime
            );

            s_dal.Courier.Create(doCourier);
            Observers.NotifyListUpdated(); // stage 5
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException($"DAL: courier with id {boCourier.Id} already exists", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: unexpected error while creating courier", ex);
        }
    }

    /// <summary>
    /// Update an existing courier via DAL.
    /// </summary>
    internal static void UpdateCourier(BO.Courier courier, BO.UserRole userRole)
    {
        var oldCourier = s_dal.Courier.Read(c => c.Id == courier.Id);
        if (oldCourier is null)
            throw new BO.BlDoesNotExistException($"DAL: courier with id {courier.Id} not found for update");
        try
        {
            var doCourier = new DO.Courier(
                oldCourier.Id,
                courier.FullName,
                courier.PhoneNumber,
                courier.Email,
                courier.Password ?? string.Empty,
                userRole == BO.UserRole.Manager ? courier.IsActive : oldCourier.IsActive,
                courier.MaxDistance <= s_dal.Config.MaxDistance ? courier.MaxDistance : oldCourier.MaximumDistance,
                (DO.ShipmentType)(int)courier.ShipmentType,
                oldCourier.StartingWork
            );

            s_dal.Courier.Update(doCourier);
            Observers.NotifyItemUpdated(courier.Id); // stage 5
            Observers.NotifyListUpdated(); // stage 5
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"DAL: courier with id {courier.Id} not found for update", ex);
        }
    }

    /// <summary>
    /// Delete courier by id.
    /// Uses extension methods for demonstration (Any).
    /// </summary>
    internal static void DeleteCourier(int id)
    {
        try
        {
            var exists = s_dal.Courier.ReadAll().Any(c => c.Id == id);
            if (!exists)
                throw new BO.BlDoesNotExistException($"DAL: courier with id {id} not found");
            var canDelete = s_dal.Delivery.Read(c => c.CourierId == id) is null;
            if (!canDelete)
                throw new BO.BlInvalidOperationException($"DAL: courier with id {id} cannot be deleted");

            s_dal.Courier.Delete(id);
            Observers.NotifyItemUpdated(id); // stage 5
            Observers.NotifyListUpdated(); // stage 5
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"DAL: courier with id {id} does not exist", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: unexpected error while deleting courier", ex);
        }
    }

    // ---- Below are several query-syntax examples (LINQ query expressions) to satisfy the requirement
    // Each one demonstrates let / group / select new / orderby usage and maps DO -> BO types.

    /// <summary>
    /// Returns summary grouped by shipment type: (ShipmentType, Count).
    /// Demonstrates grouping + select new in query syntax.
    /// </summary>
    internal static IEnumerable<(DO.ShipmentType ShipmentType, int Count)> GetCourierCountsByShipmentType()
    {
        try
        {
            var q =
                from c in s_dal.Courier.ReadAll()
                group c by c.ShipmentType
                into g // grouping
                orderby g.Key
                select new { Type = g.Key, Cnt = g.Count() }; // select new

            return q.Select(x => (x.Type, x.Cnt)); // extension Select to convert anonymous -> tuple
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: error while computing courier counts by shipment type", ex);
        }
    }

    /// <summary>
    /// Returns active courier names ordered by the length of the name.
    /// Demonstrates let + select new + orderby in query syntax.
    /// </summary>
    internal static IEnumerable<string> GetActiveCourierNamesOrderedByNameLength()
    {
        try
        {
            var q =
                from c in s_dal.Courier.ReadAll()
                where c.IsActive
                let nameLen = c.Name.Length // let usage
                orderby nameLen
                select new { c.Name, Len = nameLen }; // select new

            // extension method Select to get string list
            return q.Select(x => x.Name);
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: error while computing active courier names", ex);
        }
    }

    /// <summary>
    /// Returns couriers who started in the current year.
    /// Demonstrates query syntax with select new (mapped to BO.CourierInList).
    /// </summary>
    internal static IEnumerable<BO.CourierInList> GetCouriersStartedThisYear()
    {
        try
        {
            var year = DateTime.Now.Year;

            var q =
                from c in s_dal.Courier.ReadAll()
                where c.StartingWork.Year == year
                select new BO.CourierInList
                {
                    Id = c.Id,
                    FullName = c.Name,
                    IsActive = c.IsActive,
                    ShipmentType = (BO.ShipmentType)(int)c.ShipmentType,
                    StartWorkTime = c.StartingWork,
                    TotalDeliveredOnTime = 0,
                    TotalDeliveredLate = 0,
                    CurrentDeliveryId = null
                };

            // return as list (extension ToList)
            return q.ToList();
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: error while reading couriers started this year", ex);
        }
    }

    internal static void PeriodicCouriersUpdates()
    {
        var list = s_dal.Courier.ReadAll().ToList();
        bool courierUpdated = false; // stage 5
        foreach (var doCourier in list)
        {
            //if student study for more than MaxRange years
            //then student should be automatically updated to 'not active'
            if (AdminManager.Now - doCourier.StartingWork >= s_dal.Config.TimeToBeDispose)
            {
                s_dal.Courier.Update(doCourier with { IsActive = false });
                courierUpdated = true; // stage 5
                Observers.NotifyItemUpdated(doCourier.Id); // stage 5
            }
        }

        if (courierUpdated)
            Observers.NotifyListUpdated(); // stage 5
    }
}