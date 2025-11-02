
namespace Dal;
using DalApi;
sealed public class DalList : IDal
{
    public IConfig Config => throw new NotImplementedException();

    public ICourier Courier => throw new NotImplementedException();

    public IDelivery Delivery => throw new NotImplementedException();

    public IOrder Order => throw new NotImplementedException();

    public void ResetDB()
    {
        throw new NotImplementedException();
    }
}

