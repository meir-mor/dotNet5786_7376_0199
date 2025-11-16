
using DO;
using Helpers;
using System;

namespace BO;

/// <summary>
/// Logical entity representing an order in a general orders list (הזמנה ברשימה) - view only.
/// </summary>
/// <param name="DeliveryId">Id of current delivery (nullable if none).</param>
/// <param name="OrderId">Order id.</param>
/// <param name="OrderType">Type of order.</param>
/// <param name="AerialDistance">Aerial distance from company (computed).</param>
/// <param name="OrderStatus">Order status (computed).</param>
/// <param name="ScheduleStatus">Schedule/timeliness status (computed).</param>
/// <param name="TimeRemaining">Time remaining until max delivery time.</param>
/// <param name="TotalHandlingTime">Total time taken to complete the order (if closed).</param>
/// <param name="TotalDeliveriesCount">Total number of deliveries done for this order.</param>
public class OrderInList
{
    public int? DeliveryId { get; set; } = null;
    public int OrderId { get; init; }
    public OrderType OrderType { get; set; }
    public double AerialDistance { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public ScheduleStatus ScheduleStatus { get; set; }
    public TimeSpan TimeRemaining { get; set; }
    public TimeSpan TotalHandlingTime { get; set; }
    public int TotalDeliveriesCount { get; set; }

    /// <summary>
    /// Debug friendly string representation.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}