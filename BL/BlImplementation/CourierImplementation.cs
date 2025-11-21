using BO;

namespace BlImplementation;

using System;
using System.Linq;
using System.Collections.Generic;
using BlApi;
using Helpers;

internal class CourierImplementation : ICourier
{
    public UserRole Login(string username, string password)
    {
        // check manager credentials first
        var cfg = AdminManager.GetConfig();
        if (!string.IsNullOrWhiteSpace(username) &&
            (username == cfg.ManagerId.ToString() || username.Equals("manager", StringComparison.OrdinalIgnoreCase)) &&
            password == cfg.ManagerPassword)
            return UserRole.Manager;

        // try numeric id login for courier
        if (int.TryParse(username, out var id))
        {
            var bo = CourierManager.GetCourierById(id);
            if (bo != null && bo.Password == password)
                return UserRole.Courier;
        }

        // fallback - invalid credentials
        throw new BO.DalException("Invalid username or password");
    }

    public IEnumerable<CourierInList> GetCouriersList(int requesterId, bool? isActiveFilter = null, CourierListSortBy? sortBy = null)
    {
        // build predicate from isActiveFilter
        Func<BO.CourierInList, bool>? predicate = null;
        if (isActiveFilter.HasValue)
            predicate = c => c.IsActive == isActiveFilter.Value;

        var list = CourierManager.GetAllCouriersInList(predicate);

        // apply sorting if requested
        return sortBy switch
        {
            CourierListSortBy.FullName => list.OrderBy(c => c.FullName),
            CourierListSortBy.StartWorkTime => list.OrderBy(c => c.StartWorkTime),
            CourierListSortBy.TotalDeliveredOnTime => list.OrderByDescending(c => c.TotalDeliveredOnTime),
            CourierListSortBy.TotalDeliveredLate => list.OrderByDescending(c => c.TotalDeliveredLate),
            _ => list.OrderBy(c => c.Id)
        };
    }

    public BO.Courier GetCourierDetails(int requesterId, int courierId)
        => CourierManager.GetCourierById(courierId)
           ?? throw new BO.DalException($"Courier with id {courierId} not found");

    public void UpdateCourier(int requesterId, BO.Courier courier)
    {
        // Helper expects a DO.ShipmentType parameter in current implementation.
        // Try to preserve existing shipment type by reading the DO entry via Helpers (if needed in the future).
        // For now use a sensible default (Motorcycle) � adjust if you add shipment type to BO.Courier or to helper overloads.
        CourierManager.UpdateCourier(courier, DO.ShipmentType.Motorcycle);
    }

    public void DeleteCourier(int requesterId, int courierId)
        => CourierManager.DeleteCourier(courierId);

    public void AddCourier(int requesterId, BO.Courier courier)
    {
        // As with Update, we don't have shipment type in BO at the moment; supply default.
        CourierManager.CreateCourier(courier, DO.ShipmentType.Motorcycle);
    }
}