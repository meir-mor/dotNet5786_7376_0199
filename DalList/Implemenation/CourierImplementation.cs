namespace Dal;
using DalApi;
using DO;
using static DataSource;


public class CourierImplementation : ICourier
{
    public void Create(Courier item)
    {
        var id = Config.NextCourierId;
        Couriers.Add(item with { Id = id });
    }

    public Courier? Read(int id)
    {
        return Couriers.FirstOrDefault(c => c.Id == id, defaultValue:null);
    }

    public List<Courier> ReadAll()
    {
        return Couriers.ToList();
    }

    public void Update(Courier item)
    {
        var index = Couriers.FindIndex(c => c.Id == item.Id);
        if (index == -1)
            throw new KeyNotFoundException($"Courier with ID {item.Id} not found.");

        Couriers[index] = item;
    }

    public void Delete(int id)
    {
        Courier? courier = Couriers.FirstOrDefault(c => c.Id == id);
        if (courier == null)
            throw new KeyNotFoundException($"Courier with ID {id} not found.");

        Couriers.Remove(courier);
    }

    public void DeleteAll()
    {
        Couriers.Clear();
    }
}