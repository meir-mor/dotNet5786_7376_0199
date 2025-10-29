namespace DalApi;

public interface IConfig
{
    /// <summary>
    /// add function in the future if needed
    /// </summary>
    /// DateTime Clock { get; set; }
    int ManagerId { get; set; }

    DateTime Clock { get; set; }
    TimeSpan RiskRange { get; set; }
    string ComponyAddress { get; set; }

    void Reset();
}