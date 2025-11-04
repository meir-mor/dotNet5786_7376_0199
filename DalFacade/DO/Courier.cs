namespace DO;

/// <summary>
/// Represents a courier with personal and work-related details.
/// </summary>
/// <param name="Id">Unique courier ID.</param>
/// <param name="Name">Courier full name.</param>
/// <param name="Phone">Courier phone number.</param>
/// <param name="Email">Courier email address.</param>
/// <param name="IsActive">Indicates whether the courier is active.</param>
/// <param name="MaximumDistance">Maximum delivery distance (km).</param>
/// <param name="ShipmentType">Type of shipment handled by the courier.</param>
/// <param name="StartingWork">Date the courier started working.</param>
public record Courier(
    int Id,
    string Name,
    string Phone,
    string Email,
    string Password,
    bool IsActive,
    double? MaximumDistance,
    ShipmentType ShipmentType,
    DateTime StartingWork
) : Entity(Id)
{
    public Courier() : this(0, "", "", "", "", false, null, default, DateTime.MinValue)
    {
    }
}