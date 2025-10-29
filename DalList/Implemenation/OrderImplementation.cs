namespace Dal;
using DalApi;
using DO;
using static DataSource;

public class OrderImplementation : IOrder
{
    public void Create(Order item)
    {
        var id = Config.NextOrderId;
        Orders.Add(item with { Id = id, CreatedAt = Config.Clock });
    }

    public Order? Read(int id)
    {
        return Orders.FirstOrDefault(o => o.Id == id);
    }

    public List<Order> ReadAll()
    {
        return Orders.ToList();
    }

    public void Update(Order item)
    {
        var index = Orders.FindIndex(o => o.Id == item.Id);
        if (index == -1)
            throw new KeyNotFoundException($"Order with ID {item.Id} not found.");

        Orders[index] = item;
    }

    public void Delete(int id)
    {
       Order? order = Orders.FirstOrDefault(o => o.Id == id);
       if (order == null)
           throw new KeyNotFoundException($"Order with ID {id} not found.");

       Orders.Remove(order);
    }

    public void DeleteAll()
    {
        Orders.Clear();
    }
}