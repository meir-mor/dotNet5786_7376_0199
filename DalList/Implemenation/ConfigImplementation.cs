namespace Dal;
using DalApi;

internal class ConfigImplementation : IConfig
{
    public string ManagerId
    {
        get => Config.ManagerId;
        set => Config.ManagerId = value;
    }
    public string ComponyAddress
    {
        get => Config.ComponyAddress;
        set => Config.ComponyAddress = value;
    }
    public void Reset()
    {
        Config.Reset();
    }
}