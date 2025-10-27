
namespace DO;
/// <summary>
/// Represents a courier with personal and work-related details.
/// </summary>
/// <param name="id">Unique courier ID.</param>
/// <param name="name">Courier full name.</param>
/// <param name="phone">Courier phone number.</param>
/// <param name="email">Courier email address.</param>
/// <param name="isActive">Indicates whether the courier is active.</param>
/// <param name="TaximumDistance">Maximum delivery distance (km).</param>
/// <param name="TypeOfShipment">Type of shipment handled by the courier.</param>
/// <param name="startingWork">Date the courier started working.</param>
public record Courier
(
    int id,
    string name,
    string phone,
    string email,
    //string password,
    bool isActive,
    typeOfShipment TypeOfShipment,
    DateTime startingWork,
    double? TaximumDistance = null
)
{
    public Courier() : this(0, "", "", "", false, default, DateTime.Now, null) { }
}