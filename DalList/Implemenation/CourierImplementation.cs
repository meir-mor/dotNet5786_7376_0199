namespace Dal;

using DalApi;
using DO;
using static DataSource;

internal class CourierImplementation : ICrud<Courier>
{
    public void Create(Courier item)
    {
        if (Couriers.Any(c => c.Id == item.Id))
            throw new InvalidOperationException($"Courier with ID {item.Id} already exists");

        Couriers.Add(item);
    }

    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        return Couriers.Where(filter ?? (o => true));
    }

    public Courier? Read(Func<Courier, bool> filter)
    {
        return Couriers.FirstOrDefault(filter);
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