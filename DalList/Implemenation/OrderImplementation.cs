namespace Dal;

using DalApi;
using DO;
using System.Runtime.CompilerServices;
using static DataSource;

internal class OrderImplementation : IOrder
{
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Order item)
    {
        var id = Config.NextOrderId;
        Orders.Add(item with { Id = id });
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        return Orders.Where(filter ?? (o => true));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Order? Read(Func<Order, bool> filter)
    {
        return Orders.FirstOrDefault(filter);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Order? Read(int id)
    {
        return Orders.FirstOrDefault(o => o.Id == id);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Order item)
    {
        var index = Orders.FindIndex(o => o.Id == item.Id);
        if (index == -1)
            throw new DalDoesNotExistException($"Order with ID {item.Id} not found.");

        Orders[index] = item;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        var order = Orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            throw new DalDoesNotExistException($"Order with ID {id} not found.");

        Orders.Remove(order);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        Orders.Clear();
    }
}