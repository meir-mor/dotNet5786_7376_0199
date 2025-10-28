namespace Dal;
using DalApi;
using DO;
using static DataSource;

public class OrderImplementation : IOrder
{
    public void Create(Order item)
    {
        Orders.Add(new Order{Id = Config.NextOrderId,
            Type = item.Type,
            Description = item.Description,
            Address = item.Address,
            Latitude = item.Latitude,
            Longitude = item.Longitude,
            CustomerName = item.CustomerName,
            CustomerPhone = item.CustomerPhone,
            IsFragile = item.IsFragile,
            CreatedAt = item.CreatedAt});
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