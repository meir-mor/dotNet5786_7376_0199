using DO;

namespace Dal;
using DalApi;

sealed public class DalList : IDal
{
    public IConfig Config { get; } =  new ConfigImplementation();
    public ICrud<Courier> Courier { get; } =  new CourierImplementation();

    public ICrud<Delivery> Delivery { get; } = new DeliveryImplementation();

    public ICrud<Order> Order { get; } =  new OrderImplementation();

    public void ResetDB()
    {
        Courier?.DeleteAll();
        Delivery?.DeleteAll();
        Order?.DeleteAll();
        Config?.Reset();
        Console.WriteLine(@"All data reset successfully.");//לבדיקה אפשר להוריד
    }
}

