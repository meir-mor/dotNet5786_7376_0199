namespace Dal;
using DalApi;

internal class ConfigImplementation : IConfig
{
    public string ManagerId
    {
        get => Config.ManagerId;
        set => Config.ManagerId = value;
    }
    public DateTime Clock
    {
        get => Config.Clock;
        set => Config.Clock = value;
    }
    public TimeSpan RiskRange
    {
        get => Config.RiskRange;
        set => Config.RiskRange = value;
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