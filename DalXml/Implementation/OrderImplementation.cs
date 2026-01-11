using DalApi;
using DO;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using static System.Enum;

namespace Dal;

internal class OrderImplementation : IOrder
{
    private const string XMLFileName = Config.s_orders_xml;

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Order item)
    {
        var ordersRoot = XMLTools.LoadListFromXMLElement(XMLFileName);

        var orderElement = CreateOrderElement(item with { Id = Config.NextOrderId });

        ordersRoot.Add(orderElement);
        XMLTools.SaveListToXMLElement(ordersRoot, XMLFileName);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        var ordersRoot = XMLTools.LoadListFromXMLElement(XMLFileName);

        var orderElement = ordersRoot.Elements("Order")
            .FirstOrDefault(o => (int?)o.Element("Id") == id);

        if (orderElement == null)
            throw new DalDoesNotExistException($"Order with ID={id} not found.");

        orderElement.Remove();
        XMLTools.SaveListToXMLElement(ordersRoot, XMLFileName);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLElement(new XElement("Orders"), XMLFileName);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Order? Read(Func<Order, bool> filter)
    {
        var orders = ReadAll();
        return orders.FirstOrDefault(filter);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Order? Read(int id)
    {
        var ordersRoot = XMLTools.LoadListFromXMLElement(XMLFileName);

        var orderElement = ordersRoot.Elements("Order")
            .FirstOrDefault(o => (int?)o.Element("Id") == id);

        return orderElement == null ? null : ConvertXElementToOrder(orderElement);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        var ordersRoot = XMLTools.LoadListFromXMLElement(XMLFileName);

        var orders = from order in ordersRoot.Elements("Order")
            select ConvertXElementToOrder(order);

        return filter == null ? orders : orders.Where(filter);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Order item)
    {
        var ordersRoot = XMLTools.LoadListFromXMLElement(XMLFileName);

        var existingOrder = ordersRoot.Elements("Order")
            .FirstOrDefault(o => (int?)o.Element("Id") == item.Id);

        if (existingOrder == null)
            throw new DalDoesNotExistException($"Order with ID={item.Id} does not exist");

        existingOrder.ReplaceWith(CreateOrderElement(item));

        XMLTools.SaveListToXMLElement(ordersRoot, Config.s_orders_xml);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static XElement CreateOrderElement(Order o)
    {
        return new XElement("Order",
            new XElement("Id", o.Id),
            new XElement("Type", o.Type),
            new XElement("Description", o.Description),
            new XElement("Address", o.Address),
            new XElement("CustomerName", o.CustomerName),
            new XElement("CustomerPhone", o.CustomerPhone),
            new XElement("IsFragile", o.IsFragile),
            new XElement("CreatedAt", o.CreatedAt),
            new XElement("Status", o.Status),
            new XElement("Latitude", o.Latitude),
            new XElement("Longitude", o.Longitude)
        );
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static Order ConvertXElementToOrder(XElement o)
    {
        TryParse((string?)o.Element("Type"), out OrderType type);
        return new Order(
            (int?)o.Element("Id") ?? 0,
            type,
            (string?)o.Element("Description"),
            (string?)o.Element("Address") ?? "",
            (string?)o.Element("CustomerName") ?? "",
            (string?)o.Element("CustomerPhone") ?? "",
            (bool?)o.Element("IsFragile") ?? false,
            (DateTime?)o.Element("CreatedAt") ?? DateTime.Now,
            o.ToEnumNullable<OrderStatus>("Status") ?? OrderStatus.Open,
            (double?)o.Element("Latitude") ?? 0.0,
            (double?)o.Element("Longitude") ?? 0.0,
            (double?)o.Element("Volume") ?? 0.0
        );
    }
}