namespace Dal;

using DalApi;
using DO;
using System.Runtime.CompilerServices;
using static DataSource;

internal class DeliveryImplementation : IDelivery
{

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Delivery item)
    {
        var id = Config.NextDeliveryId;
        Deliveries.Add(item with { Id = id });
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        return Deliveries.Where(filter ?? (o => true));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        return Deliveries.FirstOrDefault(filter);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Delivery? Read(int id)
    {
        return Deliveries.FirstOrDefault(d => d.Id == id);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Delivery item)
    {
        var index = Deliveries.FindIndex(d => d.Id == item.Id);
        if (index == -1)
            throw new DalDoesNotExistException($"Delivery with ID {item.Id} not found.");

        Deliveries[index] = item;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        var delivery = Deliveries.FirstOrDefault(d => d.Id == id);
        if (delivery == null)
            throw new DalDoesNotExistException($"Delivery with ID {id} not found.");

        Deliveries.Remove(delivery);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        Deliveries.Clear();
    }
}