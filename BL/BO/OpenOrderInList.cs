using BlApi;
using DO;
using Helpers;

namespace BO;

/// <summary>
/// Logical entity representing an open order in the list of open orders for selection by couriers (הזמנה פתוחה ברשימה).
/// View-only.
/// </summary>
/// <param name="CourierId">Courier id (nullable, not shown in UI).</param>
/// <param name="OrderId">Order id.</param>
/// <param name="OrderType">Type of order.</param>
/// <param name="Volume">Volume/weight/fragility metadata (optional).</param>
/// <param name="FullAddress">Full address of the order.</param>
/// <param name="AerialDistance">Aerial distance from company (computed).</param>
/// <param name="ActualDistance">Actual distance for courier selection (nullable until computed).</param>
/// <param name="EstimatedActualTime">Estimated actual time (nullable until distance computed).</param>
/// <param name="ScheduleStatus">Timeliness status (computed).</param>
/// <param name="TimeRemaining">Time remaining until max delivery time.</param>
/// <param name="MaxDeliveryTime">Max allowed delivery time (computed).</param>
public class OpenOrderInList
{
    public int? CourierId { get; set; } = null;
    public int OrderId { get; init; }
    public OrderType OrderType { get; set; }
    public double? Volume { get; set; } = null;
    public string FullAddress { get; set; } = string.Empty;
    public double AerialDistance { get; set; }
    public double? ActualDistance { get; set; } = null;
    public TimeSpan? EstimatedActualTime { get; set; } = null;
    public ScheduleStatus ScheduleStatus { get; set; }
    public TimeSpan TimeRemaining { get; set; }
    public DateTime MaxDeliveryTime { get; set; }

    /// <summary>
    /// Debug friendly string representation.
    /// </summary>
    public override string ToString()
    {
        return this.ToStringProperty();
    }
}