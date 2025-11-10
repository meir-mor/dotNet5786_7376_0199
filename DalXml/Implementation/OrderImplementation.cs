using DO;
using DalApi;
using System.Xml.Linq;

namespace Dal;

internal class OrderImplementation : IOrder
{
    private readonly string xmlFileName = Config.s_orders_xml;

    public void Create(Order item)
    {
        var ordersRoot = XMLTools.LoadListFromXMLElement(xmlFileName);
        
        var orderElement = new XElement("Order",
            new XElement("Id", Config.NextOrderId),
            new XElement("Type", item.Type),
            new XElement("Description", item.Description),
            new XElement("Address", item.Address),
            new XElement("CustomerName", item.CustomerName),
            new XElement("CustomerPhone", item.CustomerPhone),
            new XElement("IsFragile", item.IsFragile),
            new XElement("CreatedAt", item.CreatedAt),
            new XElement("Status", item.Status),
            new XElement("Latitude", item.Latitude),
            new XElement("Longitude", item.Longitude)
        );

        ordersRoot.Add(orderElement);
        XMLTools.SaveListToXMLElement(ordersRoot, xmlFileName);
    }

    public void Delete(int id)
    {
        var ordersRoot = XMLTools.LoadListFromXMLElement(xmlFileName);

        var orderElement = ordersRoot.Elements("Order")
                                           .FirstOrDefault(o => (int?)o.Element("Id") == id);

        if (orderElement == null)
            throw new DalDoesNotExistException($"Order with ID={id} not found.");

        orderElement.Remove();
        XMLTools.SaveListToXMLElement(ordersRoot, xmlFileName);
    }

    public void DeleteAll()
    {
        XMLTools.SaveListToXMLElement(new XElement("Orders"), xmlFileName);
    }

    public Order? Read(Func<Order, bool> filter)
    {
        var orders = ReadAll();
        return orders.FirstOrDefault(filter);
    }

    public Order? Read(int id)
    {
        var ordersRoot = XMLTools.LoadListFromXMLElement(xmlFileName);

        var orderElement = ordersRoot.Elements("Order")
                                           .FirstOrDefault(o => (int?)o.Element("Id") == id);

        return orderElement == null ? null : ConvertXElementToOrder(orderElement);
    }

    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        var ordersRoot = XMLTools.LoadListFromXMLElement(xmlFileName);

        var orders = from order in ordersRoot.Elements("Order")
                     select ConvertXElementToOrder(order);

        return filter == null ? orders : orders.Where(filter);
    }

    public void Update(Order item)
    {
        var ordersRoot = XMLTools.LoadListFromXMLElement(Config.s_orders_xml);

        var existingOrder = ordersRoot.Elements("Order")
                                            .FirstOrDefault(o => (int?)o.Element("Id") == item.Id);

        if (existingOrder == null)
            throw new DalDoesNotExistException($"Order with ID={item.Id} does not exist");

        existingOrder.ReplaceWith(CreateOrderElement(item));

        XMLTools.SaveListToXMLElement(ordersRoot, Config.s_orders_xml);
    }

    public static XElement CreateOrderElement(Order o) =>
        new XElement("Order",
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

    public static Order ConvertXElementToOrder(XElement o) =>
        new Order(
            Id: (int?)o.Element("Id") ?? 0,
            Type:Enum.Parse<OrderType>((string)o.Element("Type")!),
            Description: (string?)o.Element("Description"),
            Address: (string?)o.Element("Address") ?? "",
            CustomerName: (string?)o.Element("CustomerName") ?? "",
            CustomerPhone: (string?)o.Element("CustomerPhone") ?? "",
            IsFragile: (bool?)o.Element("IsFragile") ?? false,
            CreatedAt: (DateTime?)o.Element("CreatedAt") ?? DateTime.Now,
            Status: o.ToEnumNullable<OrderStatus>("Status") ?? OrderStatus.Open,
            Latitude: (double?)o.Element("Latitude") ?? 0.0,
            Longitude: (double?)o.Element("Longitude") ?? 0.0
        );
}
