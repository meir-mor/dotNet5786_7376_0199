namespace DalApi;

public interface IConfig
{
    /// <summary>
    /// add function in the future if needed
    /// </summary>
    /// DateTime Clock { get; set; }
    string ManagerId { get; set; }

    string ComponyAddress{ get; set; }

    void Reset();
}