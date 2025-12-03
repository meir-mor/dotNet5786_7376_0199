using Helpers;

namespace BO;

/// <summary>
/// Logical entity representing a delivery entry inside an order's deliveries list (משלוח ברשימה).
/// View-only.
/// </summary>
/// <param name="DeliveryId">Delivery identifier (not shown in UI).</param>
/// <param name="CourierId">Courier id (nullable if no courier assigned).</param>
/// <param name="CourierName">Name of the courier.</param>
/// <param name="DeliveryType">Type of delivery.</param>
/// <param name="DeliveryStartTime">Time the delivery started.</param>
/// <param name="DeliveryEndType">Type of delivery completion (nullable).</param>
/// <param name="DeliveryEndTime">Delivery end time (nullable).</param>
public class DeliveryPerOrderInList
{
    public int DeliveryId { get; init; }
    public int? CourierId { get; set; } = null;
    public string CourierName { get; set; } = string.Empty;
    public ShipmentType ShipmentType { get; set; }
    public DateTime DeliveryStartTime { get; set; }
    public DeliveryEndType? DeliveryEndType { get; set; } = null;
    public DateTime? DeliveryEndTime { get; set; } = null;

    /// <summary>
    /// Debug friendly string representation.
    /// </summary>
    public override string ToString()
    {
        return this.ToStringProperty();
    }
}