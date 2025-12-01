
namespace BlApi;

/// <summary>
/// Logical service interface for order-related use cases exposed to the presentation layer.
/// Implementations must validate requesterId for authorization/audit, wrap DAL calls in try/catch,
/// translate DO exceptions into BO-level exceptions and must not hold BL-level entity collections.
/// </summary>
public interface IOrder : IObservable //stage 5
{
    /// <summary>
    /// Summary matrix of order counts by (OrderStatus x ScheduleStatus).
    /// Returned value is a 2D array int[orderStatusCount, scheduleStatusCount].
    /// Must be produced using LINQ GroupBy or query-group syntax.
    /// </summary>
    int[,] GetOrdersCountsSummary(int requesterId);

    /// <summary>
    /// Request list of orders (each order appears once, with its last delivery if exists).
    /// - filterBy: nullable enum to choose which BO.OrderInList property to filter on (if null no filter).
    /// - filterValue: nullable object - the value to compare with the selected filter property (ignored when filterBy is null).
    /// - sortBy: nullable enum - if null sort by OrderStatus; otherwise sort by the chosen property.
    /// </summary>
    IEnumerable<BO.OrderInList> GetOrdersList(int requesterId, BO.OrderListFilterBy? filterBy = null,
        object? filterValue = null, BO.OrderListSortBy? sortBy = null);

    /// <summary>
    /// Read detailed order (BO.Order) including deliveries (if any).
    /// </summary>
    BO.Order GetOrderDetails(int requesterId, int orderId);

    /// <summary>
    /// Update an existing order. BO.Order contains full values to replace existing entity.
    /// </summary>
    void UpdateOrder(int requesterId, BO.Order order);

    /// <summary>
    /// Cancel an order.
    /// If order is Open -> create a "dummy" delivery with Start==End==now, DeliveryEndType = Cancelled, CourierId = 0.
    /// If order is InProgress -> update the corresponding DO.Delivery: EndTime = now, DeliveryEndType = Cancelled.
    /// </summary>
    void CancelOrder(int requesterId, int orderId);

    /// <summary>
    /// Delete an order (not used by UI; only for tests). Implementations should throw a BO.OperationNotAllowedException
    /// that explains that deletion of orders is not permitted in the system.
    /// </summary>
    void DeleteOrder(int requesterId, int orderId);

    /// <summary>
    /// Add a new order (BO.Order). DAL creates the new order id automatically.
    /// </summary>
    void AddOrder(int requesterId, BO.Order order);

    /// <summary>
    /// Mark a delivery as completed by courier (used by courier UI).
    /// Validates that requesterId == courierId and delivery belongs to that courier.
    /// Updates DO.Delivery: EndTime = now, DeliveryEndType = Supplied.
    /// </summary>
    void CompleteDelivery(int requesterId, int courierId, int deliveryId);

    /// <summary>
    /// Assign order to courier (courier chooses an open order to handle).
    /// Validates that order is Open and not currently in-progress; creates a DO.Delivery with:
    ///   StartTime = now, EndTime = null, DeliveryEndType = null, CourierId = courierId.
    /// </summary>
    void AssignOrderToCourier(int requesterId, int courierId, int orderId);

    /// <summary>
    /// Get closed deliveries that were handled by a courier.
    /// - typeFilter: nullable to filter by BO.ShipmentType
    /// - sortBy: nullable sort key; if null sort by timeliness (ScheduleStatus)
    /// </summary>
    IEnumerable<BO.ClosedDeliveryInList> GetClosedDeliveriesForCourier(int requesterId, int courierId,
        BO.ShipmentType? typeFilter = null, BO.ClosedDeliverySortBy? sortBy = null);

    /// <summary>
    /// Return open orders available for selection by a courier.
    /// - only open orders whose aerial distance from courier <= courier.MaxDistance.
    /// - typeFilter: nullable BO.OrderType
    /// - sortBy: nullable; if null sort by timeliness (ScheduleStatus)
    /// </summary>
    IEnumerable<BO.OpenOrderInList> GetOpenOrdersForCourier(int requesterId, int courierId,
        BO.OrderType? typeFilter = null, BO.OpenOrderSortBy? sortBy = null);
}