using BlApi;
using DO;
using Helpers;

namespace BO;

/// <summary>
/// Logical entity representing an order (הזמנה) including metadata and list of deliveries for the order.
/// </summary>
/// <param name="Id">מספר מזהה רץ של ישות ההזמנה - unique order id.</param>
/// <param name="OrderType">Type of order.</param>
/// <param name="Description">Optional textual description.</param>
/// <param name="FullAddress">Full address for the order.</param>
/// <param name="Latitude">Latitude of the order address (computed by BL).</param>
/// <param name="Longitude">Longitude of the order address (computed by BL).</param>
/// <param name="AerialDistance">Aerial distance from company (computed).</param>
/// <param name="CustomerFullName">Full name of the customer.</param>
/// <param name="CustomerPhone">Customer phone number.</param>
/// <param name="Volume">Volume/weight/fragility metadata (optional).</param>
/// <param name="OrderOpenTime">Time the order was opened.</param>
/// <param name="ExpectedDeliveryTime">Expected delivery time (computed).</param>
/// <param name="MaxDeliveryTime">Max allowed delivery time (computed).</param>
/// <param name="OrderStatus">Order status (computed).</param>
/// <param name="ScheduleStatus">Schedule/timeliness status (computed).</param>
/// <param name="TimeRemaining">Time remaining until max delivery time.</param>
/// <param name="Deliveries">List of deliveries for this order (nullable if none).</param>
public class Order
{
    public int Id { get; init; }
    public OrderType OrderType { get; set; }
    public string? Description { get; set; } = null;
    public string FullAddress { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double AerialDistance { get; set; }
    public string CustomerFullName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public double Volume { get; set; }
    public DateTime OrderOpenTime { get; set; }
    public DateTime ExpectedDeliveryTime { get; set; }
    public TimeSpan MaxDeliveryTime { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public ScheduleStatus ScheduleStatus { get; set; }
    public TimeSpan TimeRemaining { get; set; }
    public List<DeliveryPerOrderInList>? Deliveries { get; set; } = null;

    /// <summary>
    /// Debug friendly string representation.
    /// </summary>
    public override string ToString()
    {
        return this.ToStringProperty();
    }
}