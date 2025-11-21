using DO;
using Helpers;

namespace BO;

/// <summary>
/// Logical entity representing a courier in list views (שליח ברשימה) - view only.
/// </summary>
/// <param name="Id">ת"ז שליח - courier identifier.</param>
/// <param name="FullName">Full name of courier.</param>
/// <param name="IsActive">Whether courier is active.</param>
/// <param name="DeliveryType">Type of delivery the courier performs.</param>
/// <param name="StartWorkTime">Time courier started at company.</param>
/// <param name="TotalDeliveredOnTime">Total delivered on time (computed).</param>
/// <param name="TotalDeliveredLate">Total delivered late (computed).</param>
/// <param name="CurrentDeliveryId">Delivery id currently in progress for this courier (nullable).</param>
public class CourierInList
{
    public int Id { get; init; }
    public string FullName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public ShipmentType shipmentType { get; set; }
    public DateTime StartWorkTime { get; set; }
    public int TotalDeliveredOnTime { get; set; }
    public int TotalDeliveredLate { get; set; }
    public int? CurrentDeliveryId { get; set; } = null;

    /// <summary>
    /// Debug friendly string representation.
    /// </summary>
    public override string ToString()
    {
        return this.ToStringProperty();
    }
}