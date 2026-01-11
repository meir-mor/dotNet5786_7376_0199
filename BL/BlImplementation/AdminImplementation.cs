using BO;

namespace BlImplementation;

using BlApi;
using Helpers;

internal class AdminImplementation : IAdmin
{
    public void AddClockObserver(Action clockObserver) =>
        AdminManager.ClockUpdatedObservers += clockObserver;

    public void RemoveClockObserver(Action clockObserver) =>
        AdminManager.ClockUpdatedObservers -= clockObserver;

    public void AddConfigObserver(Action configObserver) =>
        AdminManager.ConfigUpdatedObservers += configObserver;

    public void RemoveConfigObserver(Action configObserver) =>
        AdminManager.ConfigUpdatedObservers -= configObserver;

    public void ResetDB()
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        AdminManager.ResetDB();
    }

    public void InitializeDB()
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        AdminManager.InitializeDB();
    }

    public DateTime GetClock()
    {
        return AdminManager.Now;
    }

    public void ForwardClock(TimeUnit unit)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

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
    {
        return AdminManager.GetConfig();
    }

    public void SetConfig(BO.Config config)
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7

        AdminManager.SetConfig(config);
    }

    public void StartSimulator(int interval)  //stage 7
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        AdminManager.Start(interval); //stage 7
    }
public void StopSimulator()
    => AdminManager.Stop(); //stage 7

}