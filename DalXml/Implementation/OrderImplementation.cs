using DO;
using DalApi;

namespace Dal;

internal class OrderImplementation : IOrder
{
    private readonly string xmlFileName = Config.s_orders_xml;

    public void Create(Order item)
    {
        var orders = XMLTools.LoadListFromXMLSerializer<Order>(xmlFileName);

        var nextId = Config.NextOrderId;
        orders.Add(item with { Id = nextId });

        XMLTools.SaveListToXMLSerializer(orders, xmlFileName);
    }

    public void Delete(int id)
    {
        var orders = XMLTools.LoadListFromXMLSerializer<Order>(xmlFileName);
        var order = orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
            throw new DalDoesNotExistException($"Order with ID={id} not found.");

        orders.Remove(order);
        XMLTools.SaveListToXMLSerializer(orders, xmlFileName);
    }

    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Order>(), xmlFileName);
    }

    public Order? Read(Func<Order, bool> filter)
    {
        var orders = XMLTools.LoadListFromXMLSerializer<Order>(xmlFileName);
        return orders.FirstOrDefault(filter);
    }

    public Order? Read(int id)
    {
        var orders = XMLTools.LoadListFromXMLSerializer<Order>(xmlFileName);
        return orders.FirstOrDefault(o => o.Id == id);
    }

    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        var orders = XMLTools.LoadListFromXMLSerializer<Order>(xmlFileName);
        return filter == null ? orders : orders.Where(filter);
    }

    public void Update(Order item)
    {
        var orders = XMLTools.LoadListFromXMLSerializer<Order>(xmlFileName);

        if (orders.RemoveAll(o => o.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Order with ID={item.Id} does not exist.");

        orders.Add(item);
        XMLTools.SaveListToXMLSerializer(orders, xmlFileName);
    }
}