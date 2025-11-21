namespace DO;

/// <summary>
/// Represents a delivery entity linking an order and the courier who accepted it.
/// </summary>
/// <param name="Id">Unique delivery ID.</param>
/// <param name="OrderId">Unique ID of the order related to this delivery.</param>
/// <param name="CourierId">Courier ID (0 if this is a simulated/cancelled delivery).</param>
/// <param name="ShipmentType">Type of shipment at the time of delivery creation.</param>
/// <param name="StartTime">Date and time when the delivery started (order collected).</param>
/// <param name="ActualDistance">Actual distance (in km) between company and delivery address. Null until calculated.</param>
/// <param name="DeliveryEndType">Indicates how the delivery was completed (null if still in progress).</param>
/// <param name="EndTime">Date and time when the delivery ended (null if still active).</param>
public record Delivery(
    int Id,
    int OrderId,
    int CourierId,
    ShipmentType ShipmentType,
    DateTime StartTime,
    double? ActualDistance,
    DeliveryEndType? DeliveryEndType,
    DateTime? EndTime
)
{
    public Delivery() : this(0, 0, 0, default, DateTime.MinValue, null, null, null)
    {
    }
}