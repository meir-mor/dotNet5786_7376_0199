using BlApi;
using DO;
using Helpers;

namespace BO;

/// <summary>
/// Logical entity representing a closed delivery in a courier's closed deliveries list (משלוח סגור ברשימה).
/// View-only.
/// </summary>
/// <param name="DeliveryId">Delivery identifier (not shown in UI).</param>
/// <param name="OrderId">Order identifier.</param>
/// <param name="OrderType">Type of order.</param>
/// <param name="FullAddress">Full address of the order.</param>
/// <param name="DeliveryType">Type of delivery.</param>
/// <param name="ActualDistance">Actual distance (nullable).</param>
/// <param name="TotalHandlingTime">Total time taken to handle the delivery (difference between start and end).</param>
/// <param name="DeliveryEndType">Type of delivery completion (nullable).</param>
public class ClosedDeliveryInList
{
    public int DeliveryId { get; init; }
    public int OrderId { get; init; }
    public OrderType OrderType { get; set; }
    public string FullAddress { get; set; } = string.Empty;
    public ShipmentType shipmentType { get; set; }
    public double? ActualDistance { get; set; } = null;
    public TimeSpan TotalHandlingTime { get; set; }
    public DeliveryEndType? DeliveryEndType { get; set; } = null;

    /// <summary>
    /// Debug friendly string representation.
    /// </summary>
    public override string ToString()
    {
        return this.ToStringProperty();
    }
}