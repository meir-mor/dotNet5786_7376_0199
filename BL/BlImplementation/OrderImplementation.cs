using ShipmentType = DO.ShipmentType;

namespace BlImplementation;

using System;
using System.Linq;
using System.Collections.Generic;
using BlApi;
using Helpers;
using BO;

internal class OrderImplementation : IOrder
{
    #region Stage 5

    public void AddObserver(Action listObserver) =>
        OrderManager.Observers.AddListObserver(listObserver); //stage 5

    public void AddObserver(int id, Action observer) =>
        OrderManager.Observers.AddObserver(id, observer); //stage 5

    public void RemoveObserver(Action listObserver) =>
        OrderManager.Observers.RemoveListObserver(listObserver); //stage 5

    public void RemoveObserver(int id, Action observer) =>
        OrderManager.Observers.RemoveObserver(id, observer); //stage 5

    #endregion Stage 5

    public int[,] GetOrdersCountsSummary(int requesterId)
    {
        return OrderManager.GetOrdersCountsSummary(requesterId);
    }

    public IEnumerable<BO.OrderInList> GetOrdersList(int requesterId, BO.OrderListFilterBy? filterBy = null,
        object? filterValue = null, BO.OrderListSortBy? sortBy = null)
    {
        // start from all orders
        var orders = OrderManager.GetAllOrders().ToList();

        // filter
        if (filterBy.HasValue && filterBy.Value != BO.OrderListFilterBy.None && filterValue is not null)
            orders = filterBy.Value switch
            {
                BO.OrderListFilterBy.OrderType => orders.Where(o => o.OrderType.Equals((DO.OrderType)filterValue))
                    .ToList(),
                BO.OrderListFilterBy.CustomerFullName => orders.Where(o =>
                    o.CustomerFullName.Contains(filterValue.ToString() ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase)).ToList(),
                BO.OrderListFilterBy.OrderStatus => orders.Where(o => o.OrderStatus.Equals((DO.OrderStatus)filterValue))
                    .ToList(),
                BO.OrderListFilterBy.ShipmentType => orders.Where(o =>
                        o.Deliveries != null && o.Deliveries.Any(d => d.ShipmentType == (BO.ShipmentType)filterValue))
                    .ToList(),
                _ => orders
            };

        // sort
        orders = sortBy switch
        {
            BO.OrderListSortBy.AerialDistance => orders.OrderBy(o => o.AerialDistance).ToList(),
            BO.OrderListSortBy.OrderOpenTime => orders.OrderByDescending(o => o.OrderOpenTime).ToList(),
            BO.OrderListSortBy.CustomerFullName => orders.OrderBy(o => o.CustomerFullName).ToList(),
            _ => orders.OrderBy(o => o.OrderStatus).ToList()
        };

        return orders.Select(o => new BO.OrderInList
            {
                OrderType = o.OrderType,
                OrderStatus =
                    o.Deliveries == null || !o.Deliveries.Any()
                        ? BO.OrderStatus.Open
                        : o.Deliveries.Any(d => d.DeliveryEndType == null)
                            ? BO.OrderStatus.InProgress
                            : BO.OrderStatus.Closed,
                ShipmentType = o.Deliveries?
                                   .FirstOrDefault(d => d.DeliveryEndType == null)?.ShipmentType
                               ?? o.Deliveries?.FirstOrDefault()?.ShipmentType,
                AerialDistance = o.AerialDistance,
                ScheduleStatus = o.ScheduleStatus,
                TimeRemaining = o.TimeRemaining,
                DeliveryId = o.Deliveries?.FirstOrDefault()?.DeliveryId,
                OrderId = o.Id,
                TotalDeliveriesCount = o.Deliveries?.Count ?? 0,
                TotalHandlingTime = o.Deliveries.Where(d => d.DeliveryEndTime.HasValue)
                    .Aggregate(TimeSpan.Zero, (sum, d) => sum + (d.DeliveryEndTime.Value - d.DeliveryStartTime))
            }
        );
    }

    public BO.Order GetOrderDetails(int requesterId, int orderId)
    {
        return OrderManager.GetOrderById(orderId)
               ?? throw new BO.BlException($"Order with id {orderId} not found");
    }

    public void UpdateOrder(int requesterId, BO.Order order)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        OrderManager.UpdateOrder(order);
    }

    public void CancelOrder(int requesterId, int orderId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        OrderManager.CancelOrder(orderId);
    }

    public void DeleteOrder(int requesterId, int orderId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        OrderManager.DeleteOrder(orderId);
    }

    public void AddOrder(int requesterId, BO.Order order)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        OrderManager.CreateOrder(order);
    }

    public void CompleteDelivery(int requesterId, int courierId, int deliveryId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        OrderManager.CompleteDelivery(requesterId, courierId, deliveryId);
    }

    public void AssignOrderToCourier(int requesterId, int courierId, int orderId)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        OrderManager.AssignOrderToCourier(requesterId, courierId, orderId);
    }

    public IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesForCourier(int requesterId, int courierId,
        BO.ShipmentType? typeFilter = null, BO.ClosedDeliverySortBy? sortBy = null)
    {
        return OrderManager.GetClosedDeliveriesForCourier(requesterId, courierId, typeFilter, sortBy);
    }

    public async Task<IEnumerable<BO.OpenOrderInList>> GetOpenOrdersForCourier(  int requesterId, int courierId,
       BO.OrderType? typeFilter = null, BO.OpenOrderSortBy? sortBy = null)
    {
        return await OrderManager.GetOpenOrdersForCourier(requesterId, courierId, typeFilter, sortBy);
    }

}