
using DO;
using Helpers;

namespace BO;

/// <summary>
/// Logical entity representing a courier (שליח), including personal details and current order in progress (if any).
/// </summary>
/// <param name="Id">ת"ז שליח - Unique identifier for the courier.</param>
/// <param name="FullName">Full name (first and last) of the courier.</param>
/// <param name="PhoneNumber">Mobile phone number of the courier.</param>
/// <param name="Email">Email address of the courier.</param>
/// <param name="Password">Password for courier (non-null for those who set it).</param>
/// <param name="IsActive">Whether the courier is active.</param>
/// <param name="MaxDistance">Maximum personal distance for deliveries (nullable).</param>
/// <param name="DeliveryType">Type of delivery the courier performs.</param>
/// <param name="StartWorkTime">Time the courier started working at company (view only).</param>
/// <param name="TotalDeliveredOnTime">Total orders delivered on time (view only).</param>
/// <param name="TotalDeliveredLate">Total orders delivered late (view only).</param>
/// <param name="OrderInProgress">Current order in progress for this courier (nullable, view only).</param>
public class Courier
{
    public int Id { get; init; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; } = null;
    public bool IsActive { get; set; } = true;
    public double? MaxDistance { get; set; } = null;
    public ShipmentType ShipmentType { get; set; }
    public DateTime StartWorkTime { get; set; }
    public int TotalDeliveredOnTime { get; set; }
    public int TotalDeliveredLate { get; set; }
    public OrderInProgress? OrderInProgress { get; set; } = null;

    /// <summary>
    /// Debug friendly string representation.
    /// </summary>
    public override string ToString() => this.ToStringProperty();
}