namespace Dal;

using DalApi;
using DO;

internal class CourierImplementation : ICourier
{
    private readonly string xmlFileName = Config.s_couriers_xml;

    public void Create(Courier item)
    {
        var couriers = XMLTools.LoadListFromXMLSerializer<Courier>(xmlFileName);
        if (couriers.Any(c => item.Id == c.Id))
            throw new DalAlreadyExistsException($"Courier with ID={item.Id} already exists");
        couriers.Add(item);
        XMLTools.SaveListToXMLSerializer(couriers, xmlFileName);
    }

    public void Delete(int id)
    {
        var couriers = XMLTools.LoadListFromXMLSerializer<Courier>(xmlFileName);
        var courier = couriers.FirstOrDefault(c => c.Id == id);
        if (courier == null)
            throw new DalDoesNotExistException($"Courier with ID {id} not found.");
        couriers.Remove(courier);
        XMLTools.SaveListToXMLSerializer(couriers, xmlFileName);
    }

    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Courier>(), xmlFileName);
    }

    public Courier? Read(Func<Courier, bool> filter)
    {
        var couriers = XMLTools.LoadListFromXMLSerializer<Courier>(xmlFileName);
        return couriers.FirstOrDefault(filter);
    }

    public Courier? Read(int id)
    {
        var couriers = XMLTools.LoadListFromXMLSerializer<Courier>(xmlFileName);
        if (couriers.Any(c => id == c.Id))
            throw new DalAlreadyExistsException($"Courier with ID={id} already exists");
        return couriers.FirstOrDefault(c => id == c.Id);
    }

    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        var couriers = XMLTools.LoadListFromXMLSerializer<Courier>(xmlFileName);
        return couriers.Where(filter ?? (o => true));
    }

    public void Update(Courier item)
    {
        var couriers = XMLTools.LoadListFromXMLSerializer<Courier>(xmlFileName);
        if (couriers.RemoveAll(c => c.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Courier with ID={item.Id} does Not exist");

        couriers.Add(item);
        XMLTools.SaveListToXMLSerializer(couriers, xmlFileName);
    }
}