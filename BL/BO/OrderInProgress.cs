using Helpers;

namespace BO;

/// <summary>
/// Logical entity representing an order currently in progress for a courier (הזמנה בטיפול שליח).
/// View-only entity assembled from DO.Delivery and DO.Order data.
/// </summary>
/// <param name="DeliveryId">Identifier of the delivery (not shown in UI).</param>
/// <param name="OrderId">Identifier of the order.</param>
/// <param name="OrderType">Type of the order.</param>
/// <param name="Description">Optional textual description of the order.</param>
/// <param name="FullAddress">Full address of the order.</param>
/// <param name="AerialDistance">Aerial distance from company to order (computed).</param>
/// <param name="ActualDistance">Actual distance (nullable) from delivery record.</param>
/// <param name="CustomerFullName">Full name of the customer.</param>
/// <param name="CustomerPhone">Customer phone number.</param>
/// <param name="OrderOpenTime">Time the order was opened.</param>
/// <param name="DeliveryStartTime">Time the delivery started.</param>
/// <param name="ExpectedDeliveryTime">Expected delivery time (computed).</param>
/// <param name="MaxDeliveryTime">Max allowed delivery time (computed).</param>
/// <param name="OrderStatus">Order status (computed).</param>
/// <param name="ScheduleStatus">Schedule/timeliness status (computed).</param>
/// <param name="TimeRemaining">Time remaining until max delivery time.</param>
public class OrderInProgress
{
    public int DeliveryId { get; init; }
    public int OrderId { get; init; }
    public OrderType OrderType { get; set; }
    public string? Description { get; set; } = null;
    public string FullAddress { get; set; } = string.Empty;
    public double AerialDistance { get; set; }
    public double? ActualDistance { get; set; } = null;
    public string CustomerFullName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public DateTime OrderOpenTime { get; set; }
    public DateTime DeliveryStartTime { get; set; }
    public DateTime ExpectedDeliveryTime { get; set; }
    public DateTime MaxDeliveryTime { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public ScheduleStatus ScheduleStatus { get; set; }
    public TimeSpan TimeRemaining { get; set; }

    /// <summary>
    /// Debug friendly string representation.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}