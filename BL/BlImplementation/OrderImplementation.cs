using BO;
using DO;

namespace BlImplementation;

using System;
using System.Linq;
using System.Collections.Generic;
using BlApi;
using Helpers;

internal class OrderImplementation : IOrder
{
    public int[,] GetOrdersCountsSummary(int requesterId)
        => OrderManager.GetOrdersCountsSummary(requesterId);

    public IEnumerable<BO.OrderInList> GetOrdersList(int requesterId, OrderListFilterBy? filterBy = null, object? filterValue = null, OrderListSortBy? sortBy = null)
    {
        // start from all orders
        var orders = OrderManager.GetAllOrders().ToList();

        // filter
        if (filterBy.HasValue && filterBy.Value != OrderListFilterBy.None && filterValue is not null)
        {
            orders = filterBy.Value switch
            {
                OrderListFilterBy.OrderType => orders.Where(o => o.OrderType.Equals((OrderType)filterValue)).ToList(),
                OrderListFilterBy.CustomerFullName => orders.Where(o => o.CustomerFullName.Contains(filterValue.ToString() ?? string.Empty, StringComparison.OrdinalIgnoreCase)).ToList(),
                OrderListFilterBy.OrderStatus => orders.Where(o => o.OrderStatus.Equals((OrderStatus)filterValue)).ToList(),
                _ => orders
            };
        }

        // sort
        orders = sortBy switch
        {
            OrderListSortBy.AerialDistance => orders.OrderBy(o => o.AerialDistance).ToList(),
            OrderListSortBy.OrderOpenTime => orders.OrderByDescending(o => o.OrderOpenTime).ToList(),
            OrderListSortBy.CustomerFullName => orders.OrderBy(o => o.CustomerFullName).ToList(),
            _ => orders.OrderBy(o => o.OrderStatus).ToList()
        };

        return orders.Select(o => new BO.OrderInList
        {
            OrderType = o.OrderType,
            OrderStatus = o.OrderStatus,
            AerialDistance = o.AerialDistance,
            ScheduleStatus = o.ScheduleStatus,
            TimeRemaining = o.TimeRemaining,
            DeliveryId = o.Deliveries?.FirstOrDefault()?.DeliveryId,
            OrderId = o.Id,
            TotalDeliveriesCount = o.Deliveries?.Count ?? 0,
            TotalHandlingTime = TimeSpan.Zero
        });
    }

    public BO.Order GetOrderDetails(int requesterId, int orderId)
        => OrderManager.GetOrderById(orderId)
           ?? throw new BO.DalException($"Order with id {orderId} not found");

    public void UpdateOrder(int requesterId, BO.Order order)
    {
        OrderManager.UpdateOrder(order);
    }

    public void CancelOrder(int requesterId, int orderId)
    {
        OrderManager.CancelOrder(orderId);
    }

    public void DeleteOrder(int requesterId, int orderId)
    {
        OrderManager.DeleteOrder(orderId);
    }

    public void AddOrder(int requesterId, BO.Order order)
        => OrderManager.CreateOrder(order);

    public void CompleteDelivery(int requesterId, int courierId, int deliveryId)
        => OrderManager.CompleteDelivery(requesterId, courierId, deliveryId);

    public void AssignOrderToCourier(int requesterId, int courierId, int orderId)
        => OrderManager.AssignOrderToCourier(requesterId, courierId, orderId);

    public IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesForCourier(int requesterId, int courierId, ShipmentType? typeFilter = null, ClosedDeliverySortBy? sortBy = null)
        => OrderManager.GetClosedDeliveriesForCourier(requesterId, courierId, typeFilter, sortBy);

    public IEnumerable<BO.OpenOrderInList> GetOpenOrdersForCourier(int requesterId, int courierId, OrderType? typeFilter = null, OpenOrderSortBy? sortBy = null)
        => OrderManager.GetOpenOrdersForCourier(requesterId, courierId, typeFilter, sortBy);
}