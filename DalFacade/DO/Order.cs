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
/// <param name="Status">Order status.</param>
public record Order(
    int Id,
    OrderType Type,
    string? Description,
    string Address,
    string CustomerName,
    string CustomerPhone,
    bool IsFragile, //אולי לשנות להאם דחוף או לא, יותר שימושי...
    DateTime CreatedAt,
    OrderStatus Status,
    double Latitude,
    double Longitude
)
{
    public Order() : this(0, default, null, "", "", "", false, DateTime.Now, OrderStatus.Open, 0.0, 0.0)
    {
    }
}