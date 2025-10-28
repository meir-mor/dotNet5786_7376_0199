namespace Dal;
using DalApi;
using DO;
using static DataSource;


public class CourierImplementation : ICourier
{
    public void Create(Courier item)
    {
        Couriers.Add(new Courier{id = Config.NextCourierId,
            name = item.name,
            phone = item.phone,
            email = item.email,
            isActive = item.isActive,
            TaximumDistance = item.TaximumDistance,
            TypeOfShipment = item.TypeOfShipment,
            startingWork = item.startingWork});
    }

    public Courier? Read(int id)
    {
        return Couriers.FirstOrDefault(c => c.id == id, defaultValue:null);
    }

    public List<Courier> ReadAll()
    {
        return Couriers.ToList();
    }

    public void Update(Courier item)
    {
        var index = Couriers.FindIndex(c => c.id == item.id);
        if (index == -1)
            throw new KeyNotFoundException($"Courier with ID {item.id} not found.");

        Couriers[index] = item;
    }

    public void Delete(int id)
    {
        Courier? courier = Couriers.FirstOrDefault(c => c.id == id);
        if (courier == null)
            throw new KeyNotFoundException($"Courier with ID {id} not found.");

        Couriers.Remove(courier);
    }

    public void DeleteAll()
    {
        Couriers.Clear();
    }
}