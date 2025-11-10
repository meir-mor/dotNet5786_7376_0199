using DO;
using DalApi;

namespace Dal;

internal class DeliveryImplementation : IDelivery
{
    private readonly string xmlFileName = Config.s_deliveries_xml;

    public void Create(Delivery item)
    {
        var deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(xmlFileName);

        var nextId = Config.NextDeliveryId;
        deliveries.Add(item with { Id = nextId });

        XMLTools.SaveListToXMLSerializer(deliveries, xmlFileName);
    }

    public void Delete(int id)
    {
        var deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(xmlFileName);
        var delivery = deliveries.FirstOrDefault(d => d.Id == id);
        if (delivery == null)
            throw new DalDoesNotExistException($"Delivery with ID={id} not found.");

        deliveries.Remove(delivery);
        XMLTools.SaveListToXMLSerializer(deliveries, xmlFileName);
    }

    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Delivery>(), xmlFileName);
    }

    public Delivery? Read(Func<Delivery, bool> filter)
    {
        var deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(xmlFileName);
        return deliveries.FirstOrDefault(filter);
    }

    public Delivery? Read(int id)
    {
        var deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(xmlFileName);
        return deliveries.FirstOrDefault(d => d.Id == id);
    }

    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        var deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(xmlFileName);
        return filter == null ? deliveries : deliveries.Where(filter);
    }

    public void Update(Delivery item)
    {
        var deliveries = XMLTools.LoadListFromXMLSerializer<Delivery>(xmlFileName);

        if (deliveries.RemoveAll(d => d.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Delivery with ID={item.Id} does Not exist");

        deliveries.Add(item);
        XMLTools.SaveListToXMLSerializer(deliveries, xmlFileName);
    }
}