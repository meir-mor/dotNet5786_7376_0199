using DO;

namespace DalApi;

public interface IDal
{
    IConfig Config { get; }

    ICrud<Courier> Courier { get; }

    ICrud<Delivery> Delivery { get; }

    ICrud<Order> Order { get; }

    void ResetDB();

}
