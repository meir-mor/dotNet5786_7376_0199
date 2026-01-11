namespace Dal;

using DalApi;
using DO;
using System.Runtime.CompilerServices;
using static DataSource;

internal class CourierImplementation : ICourier
{
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Courier item)
    {
        if (Couriers.Any(c => c.Id == item.Id))
            throw new DalAlreadyExistsException($"Courier with ID={item.Id} already exists");
        Couriers.Add(item);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        return Couriers.Where(filter ?? (o => true));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(Func<Courier, bool> filter)
    {
        return Couriers.FirstOrDefault(filter);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(int id)
    {
        return Couriers.FirstOrDefault(c => c.Id == id);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Courier item)
    {
        var index = Couriers.FindIndex(c => c.Id == item.Id);
        if (index == -1)
            throw new DalDoesNotExistException($"Courier with ID {item.Id} not found.");

        Couriers[index] = item;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        var courier = Couriers.FirstOrDefault(c => c.Id == id);
        if (courier == null)
            throw new DalDoesNotExistException($"Courier with ID {id} not found.");

        Couriers.Remove(courier);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        Couriers.Clear();
    }
}