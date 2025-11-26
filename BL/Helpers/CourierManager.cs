using DalApi;
//using BlApi;

namespace Helpers;

internal static class CourierManager
{
    // DAL access (private field as requested)
    private static IDal s_dal = Factory.Get; //stage 4

    /// <summary>
    /// Return all couriers converted to BO.CourierInList.
    /// Uses LINQ extension methods (Where, Select, OrderBy, ToList).
    /// Wrapped in try/catch to translate DO exceptions into BO.DalException.
    /// </summary>
    internal static IEnumerable<BO.CourierInList> GetAllCouriersInList(Func<BO.CourierInList, bool>? filter = null)
    {
        try
        {
            var deliveries = s_dal.Delivery.ReadAll();
            var config = s_dal.Config;
            // ReadAll from DAL (returns IEnumerable&lt;DO.Courier&gt;)
            var result = s_dal.Courier.ReadAll()
                .Where(c => c != null) // extension method 1 (Where)
                .Select(c => new BO.CourierInList // extension method 2 (Select)
                {
                    Id = c.Id,
                    FullName = c.Name,
                    IsActive = c.IsActive,
                    shipmentType = c.ShipmentType,
                    StartWorkTime = c.StartingWork,
                    TotalDeliveredOnTime = deliveries.Count(d =>
                        d.CourierId == c.Id && d.DeliveryEndType == DO.DeliveryEndType.Supplied &&
                        d.EndTime - d.StartTime <= config.MaxDeliveryTime),
                    TotalDeliveredLate = deliveries.Count(d =>
                        d.CourierId == c.Id && d.DeliveryEndType == DO.DeliveryEndType.Supplied &&
                        d.EndTime - d.StartTime > config.MaxDeliveryTime),
                    CurrentDeliveryId = deliveries.Count(d => d.CourierId == c.Id && d.EndTime == null)
                })
                .OrderBy(c => c.FullName) // extension method 3 (OrderBy)
                .ToList(); // extension method 4 (ToList)

            return filter == null ? result : result.Where(filter);
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
    /// Read a single courier by id and map to BO.Courier.
    /// Uses extension method FirstOrDefault.
    /// </summary>
    internal static BO.Courier? GetCourierById(int id)
    {
        try
        {
            var d = s_dal.Courier.Read(id); // may return null

            if (d == null) return null;

            // map DO.Courier -> BO.Courier
            var bo = new BO.Courier
            {
                Id = d.Id,
                FullName = d.Name,
                PhoneNumber = d.Phone,
                Email = d.Email,
                Password = d.Password,
                IsActive = d.IsActive,
                MaxDistance = d.MaximumDistance ?? null,
                ShipmentType = d.ShipmentType,
                StartWorkTime = d.StartingWork,
                TotalDeliveredOnTime = 0,
                TotalDeliveredLate = 0,
                OrderInProgress = null
            };

            return bo;
        }
        catch (DO.DalDoesNotExistException ex)
        {
            throw new BO.BlDoesNotExistException($"DAL: courier with id {id} does not exist", ex);
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
                boCourier.ShipmentType,
                boCourier.StartWorkTime
            );

            s_dal.Courier.Create(doCourier);
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
    internal static void UpdateCourier(BO.Courier courier, UserRole userRole)
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
                userRole == UserRole.Manager ? courier.IsActive : oldCourier.IsActive,
                courier.MaxDistance,
                courier.ShipmentType,
                oldCourier.StartingWork
            );

            s_dal.Courier.Update(doCourier);
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
                    shipmentType = c.ShipmentType,
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

    internal static void PeriodicCouriersUpdates(DateTime oldClock, DateTime newClock)
    {
    }
}