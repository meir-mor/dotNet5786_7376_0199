using BO;
using DalApi;
using DO;

namespace Helpers;

internal static class DeliveryManager
{
    internal static ObserverManager Observers = new(); //stage 5  

    private static IDal s_dal = Factory.Get;

    /// <summary>
    /// Return all deliveries mapped to BO.DeliveryPerOrderInList.
    /// Uses LINQ extension methods (Where, Select, OrderBy, ToList).
    /// </summary>
    internal static IEnumerable<BO.DeliveryPerOrderInList> GetAllDeliveries(
        Func<BO.DeliveryPerOrderInList, bool>? filter = null)
    {
        try
        {
            var list = s_dal.Delivery.ReadAll()
                .Where(d => d != null) // extension method 1
                .Select(d => MapDoToBo(d)) // extension method 2
                .OrderBy(x => x.DeliveryStartTime) // extension method 3
                .ToList(); // extension method 4

            return filter == null ? list : list.Where(filter);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException("DAL: delivery not found", ex);
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("DAL: delivery already exists", ex);
        }
        catch (DO.DalNullException ex)
        {
            throw new BO.BlInvalidOperationException("DAL: null encountered in delivery", ex);
        }
        catch (DO.DalXMLFileLoadCreateException ex)
        {
            throw new BO.BlDalFileException("DAL: XML file load/create error for deliveries", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: unexpected error while reading deliveries", ex);
        }
    }

    /// <summary>
    /// Returns deliveries for a given order id (query syntax example with let & select new).
    /// </summary>
    internal static IEnumerable<BO.DeliveryPerOrderInList> GetDeliveriesForOrder(int orderId)
    {
        try
        {
            var q =
                from d in s_dal.Delivery.ReadAll()
                where d.OrderId == orderId
                let courier = s_dal.Courier.Read(d.CourierId) // let usage
                orderby d.StartTime
                select new BO.DeliveryPerOrderInList // select new usage
                {
                    DeliveryId = d.Id,
                    CourierId = d.CourierId == 0 ? null : (int?)d.CourierId,
                    CourierName = courier is null ? string.Empty : courier.Name,
                    ShipmentType = (BO.ShipmentType)Enum.Parse(typeof(BO.ShipmentType), d.ShipmentType.ToString()),
                    DeliveryStartTime = d.StartTime,
                    DeliveryEndType = d.DeliveryEndType is null
                        ? null
                        : (BO.DeliveryEndType?)(int)d.DeliveryEndType,
                    DeliveryEndTime = d.EndTime is DateTime time && time != DateTime.MinValue ? d.EndTime : null
                };

            return q.ToList();
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: error while getting deliveries for order", ex);
        }
    }

    /// <summary>
    /// Read a single delivery by id and map to BO.DeliveryPerOrderInList.
    /// </summary>
    internal static BO.DeliveryPerOrderInList? GetDeliveryById(int id)
    {
        try
        {
            var d = s_dal.Delivery.Read(id);
            return d is null ? null : MapDoToBo(d);
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"DAL: delivery {id} does not exist", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: unexpected error while reading delivery", ex);
        }
    }

    /// <summary>
    /// Create a delivery from a BO.DeliveryPerOrderInList (maps BO -> DO and calls DAL).
    /// </summary>
    internal static void CreateDelivery(BO.DeliveryPerOrderInList bo)
    {
        try
        {
            var doDelivery = new DO.Delivery(
                0,
                bo.DeliveryId == 0 ? 0 : bo.DeliveryId, // DAL implementations set id; pass 0 or given
                bo.CourierId ?? 0,
                (DO.ShipmentType)Enum.Parse(typeof(DO.ShipmentType), bo.ShipmentType.ToString()),
                bo.DeliveryStartTime,
                null, // ActualDistance not provided by BO.DeliveryPerOrderInList
                bo.DeliveryEndType is null
                    ? null
                    : (DO.DeliveryEndType?)(int)bo.DeliveryEndType,
                bo.DeliveryEndTime
            );

            s_dal.Delivery.Create(doDelivery);
            Observers.NotifyItemUpdated(doDelivery.Id);
            Observers.NotifyListUpdated();
        }
        catch (DO.DalAlreadyExistsException ex)
        {
            throw new BO.BlAlreadyExistsException("DAL: delivery already exists", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: unexpected error while creating delivery", ex);
        }
    }

    /// <summary>
    /// Update delivery (maps BO -> DO and calls DAL.Update).
    /// </summary>
    internal static void UpdateDelivery(BO.DeliveryPerOrderInList bo)
    {
        try
        {
            var doDelivery = new DO.Delivery(
                bo.DeliveryId,
                0, // OrderId unknown in BO.DeliveryPerOrderInList; DAL will expect correct value in real usage
                bo.CourierId ?? 0,
                (DO.ShipmentType)(int)bo.ShipmentType,
                bo.DeliveryStartTime,
                null,
                bo.DeliveryEndType is null
                    ? null
                    : (DO.DeliveryEndType?)(int)bo.DeliveryEndType,
                bo.DeliveryEndTime
            );

            s_dal.Delivery.Update(doDelivery);
            Observers.NotifyItemUpdated(doDelivery.Id);
            Observers.NotifyListUpdated();
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"DAL: delivery {bo.DeliveryId} not found", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: unexpected error while updating delivery", ex);
        }
    }

    /// <summary>
    /// Delete delivery by id.
    /// Demonstrates extension Any() before delete.
    /// </summary>
    internal static void DeleteDelivery(int id)
    {
        try
        {
            var exists = s_dal.Delivery.ReadAll().Any(d => d.Id == id);
            if (!exists) throw new BO.BlException($"DAL: delivery {id} not found");

            s_dal.Delivery.Delete(id);
            Observers.NotifyItemUpdated(id);
            Observers.NotifyListUpdated();
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"DAL: delivery {id} does not exist", ex);
        }
        catch (Exception ex)
        {
            throw new BO.BlUnexpectedException("DAL: unexpected error while deleting delivery", ex);
        }
    }

    internal static void PeriodicDeliveriesUpdates(DateTime oldClock, DateTime newClock)
    {
        var deliveries = s_dal.Delivery.ReadAll(d => d.DeliveryEndType == null && d.EndTime == null).ToList();
        var updated = false; // stage 5
        foreach (var doDelivery in deliveries)
        {
            if (AdminManager.Now - doDelivery.StartTime >= s_dal.Config.MaxDeliveryTime)
            {
                updated = true;
                Observers.NotifyItemUpdated(doDelivery.Id);
            }
        }

        if (updated || oldClock != newClock)
            Observers.NotifyListUpdated();
    }

    private static BO.DeliveryPerOrderInList MapDoToBo(DO.Delivery d)
    {
        return new BO.DeliveryPerOrderInList
        {
            DeliveryId = d.Id,
            CourierId = d.CourierId == 0 ? null : d.CourierId,
            CourierName = s_dal.Courier.Read(d.CourierId)?.Name,
            ShipmentType = (BO.ShipmentType)Enum.Parse(typeof(BO.ShipmentType), d.ShipmentType.ToString()),
            DeliveryStartTime = d.StartTime,
            DeliveryEndType = d.DeliveryEndType is null
                ? null
                : (BO.DeliveryEndType?)(int)d.DeliveryEndType,
            DeliveryEndTime = d.EndTime is DateTime time && time != DateTime.MinValue ? d.EndTime : null
        };
    }
}