namespace DalApi;

public interface IDal
{
    IConfig Config { get; }

    ICourier Courier { get; }

    IDelivery Delivery { get; }

    IOrder Order { get; }

    void ResetDB();
}