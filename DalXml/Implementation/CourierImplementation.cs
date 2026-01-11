namespace Dal;

using DalApi;
using DO;
using System.Runtime.CompilerServices;

internal class CourierImplementation : ICourier
{
    private readonly string xmlFileName = Config.s_couriers_xml;

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Create(Courier item)
    {
        var couriers = XMLTools.LoadListFromXMLSerializer<Courier>(xmlFileName);
        if (couriers.Any(c => item.Id == c.Id))
            throw new DalAlreadyExistsException($"Courier with ID={item.Id} already exists");
        couriers.Add(item);
        XMLTools.SaveListToXMLSerializer(couriers, xmlFileName);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Delete(int id)
    {
        var couriers = XMLTools.LoadListFromXMLSerializer<Courier>(xmlFileName);
        var courier = couriers.FirstOrDefault(c => c.Id == id);
        if (courier == null)
            throw new DalDoesNotExistException($"Courier with ID {id} not found.");
        couriers.Remove(courier);
        XMLTools.SaveListToXMLSerializer(couriers, xmlFileName);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Courier>(), xmlFileName);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(Func<Courier, bool> filter)
    {
        var couriers = XMLTools.LoadListFromXMLSerializer<Courier>(xmlFileName);
        return couriers.FirstOrDefault(filter);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public Courier? Read(int id)
    {
        var couriers = XMLTools.LoadListFromXMLSerializer<Courier>(xmlFileName);
        return couriers.FirstOrDefault(c => id == c.Id);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        var couriers = XMLTools.LoadListFromXMLSerializer<Courier>(xmlFileName);
        return couriers.Where(filter ?? (o => true));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Update(Courier item)
    {
        var couriers = XMLTools.LoadListFromXMLSerializer<Courier>(xmlFileName);
        if (couriers.RemoveAll(c => c.Id == item.Id) == 0)
            throw new DalDoesNotExistException($"Courier with ID={item.Id} does Not exist");

        couriers.Add(item);
        XMLTools.SaveListToXMLSerializer(couriers, xmlFileName);
    }
}