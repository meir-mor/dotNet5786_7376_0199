namespace BlApi;

/// <summary>
/// Logical service interface for delivery-related use cases exposed to the presentation layer.
/// Implementations must validate requesterId for authorization/audit, wrap DAL calls in try/catch,
/// translate DO exceptions into BO-level exceptions and must not hold BL-level entity collections.
/// </summary>
public interface IDelivery
{
   
    /// <summary>
    /// Return deliveries mapped to BO.DeliveryPerOrderInList.
    /// - filterBy & filterValue: if filterBy is null or None no filtering is applied.
    /// - sortBy: if null default sort is by StartTime.
    /// </summary>
    IEnumerable<BO.DeliveryPerOrderInList> GetDeliveriesList(int requesterId, DeliveryListFilterBy? filterBy = null, object? filterValue = null, DeliveryListSortBy? sortBy = null);

    /// <summary>
    /// Returns deliveries for a specific order (each delivery item in BO.DeliveryPerOrderInList).
    /// </summary>
    IEnumerable<BO.DeliveryPerOrderInList> GetDeliveriesForOrder(int requesterId, int orderId);

    /// <summary>
    /// Read a single delivery by id and map to BO.DeliveryPerOrderInList.
    /// </summary>
    BO.DeliveryPerOrderInList? GetDeliveryDetails(int requesterId, int deliveryId);

    /// <summary>
    /// Create a new delivery from a BO.DeliveryPerOrderInList (maps BO -> DO and calls DAL).
    /// </summary>
    void AddDelivery(int requesterId, BO.DeliveryPerOrderInList delivery);

    /// <summary>
    /// Update an existing delivery (maps BO -> DO).
    /// </summary>
    void UpdateDelivery(int requesterId, BO.DeliveryPerOrderInList delivery);

    /// <summary>
    /// Delete delivery by id.
    /// </summary>
    void DeleteDelivery(int requesterId, int deliveryId);
}