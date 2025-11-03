namespace Dal;

using DalApi;
using DO;
using static DataSource;

internal class CourierImplementation : ICourier
{
    public void Create(Courier item)
    {
        if (Couriers.Any(c => c.Id == item.Id))
            throw new InvalidOperationException($"Courier with ID {item.Id} already exists");

        Couriers.Add(item);
    }

    public Courier? Read(int id)
    {
        return Couriers.FirstOrDefault(c => c.Id == id, null);
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
        var courier = Couriers.FirstOrDefault(c => c.Id == id);
        if (courier == null)
            throw new KeyNotFoundException($"Courier with ID {id} not found.");

        Couriers.Remove(courier);
    }

    public void DeleteAll()
    {
        Couriers.Clear();
    }
}