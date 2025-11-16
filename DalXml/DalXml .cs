
using DalApi;
namespace Dal;

sealed internal class DalXml : IDal
{
    public IConfig Config { get; } = new ConfigImplementation();
    public ICourier Courier { get; } = new CourierImplementation();

    public IDelivery Delivery { get; } = new DeliveryImplementation();

    public IOrder Order { get; } = new OrderImplementation();

    public void ResetDB()
    {
        Courier?.DeleteAll();
        Delivery?.DeleteAll();
        Order?.DeleteAll();
        Config?.Reset();
        Console.WriteLine(@"All data reset successfully.");
    }
}
