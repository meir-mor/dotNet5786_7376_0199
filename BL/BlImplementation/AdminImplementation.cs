namespace BlImplementation;

using BlApi;
using Helpers;

internal class AdminImplementation : IAdmin
{
    public void ResetDB()
        => AdminManager.ResetDB();

    public void InitializeDB()
        => AdminManager.InitializeDB();

    public DateTime GetClock()
        => AdminManager.Now;

    public void ForwardClock(TimeUnit unit)
    {
        var now = AdminManager.Now;
        var newTime = unit switch
        {
            TimeUnit.Minute => now.AddMinutes(1),
            TimeUnit.Hour => now.AddHours(1),
            TimeUnit.Day => now.AddDays(1),
            TimeUnit.Month => now.AddMonths(1),
            TimeUnit.Year => now.AddYears(1),
            _ => now
        };

        AdminManager.UpdateClock(newTime);
    }

    public BO.Config GetConfig()
        => AdminManager.GetConfig();

    public void SetConfig(BO.Config config)
        => AdminManager.SetConfig(config);
}