
namespace DO;
/// <summary>
/// 
/// </summary>
/// <param name="id"></param>
/// <param name="name"></param>
/// <param name="phone"></param>
/// <param name="email"></param>
/// <param name="isActive"></param>
/// <param name="TaximumDistance"></param>
/// <param name="TypeOfShipment"></param>
/// <param name="startingWork"></param>
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
public Courier() : this(0) { }
}