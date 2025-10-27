
namespace DO;

/// <summary>
/// Represents a customer order with delivery and item details.
/// </summary>
/// <param name="Id">Unique order ID.</param>
/// <param name="Type">Order type.</param>
/// <param name="Description">Optional order description.</param>
/// <param name="Address">Full delivery address.</param>
/// <param name="Latitude">Delivery location latitude.</param>
/// <param name="Longitude">Delivery location longitude.</param>
/// <param name="CustomerName">Customer full name.</param>
/// <param name="CustomerPhone">Customer phone number.</param>
/// <param name="IsFragile">Indicates whether the package is fragile.</param>
/// <param name="CreatedAt">Date and time when the order was created.</param>
public record Order
(
     int Id,
    OrderType Type,
    string? Description,
    string Address,
    double Latitude,
    double Longitude,
    string CustomerName,
    string CustomerPhone,
    bool IsFragile,
    DateTime CreatedAt
)
{
public Order(): this(0, default, null, "", 0.0, 0.0, "", "", false, DateTime.Now) { }
}
