//using BO;

using System.Runtime.CompilerServices;

namespace Helpers;

/// <summary>
/// Internal BL manager for all Application's Configuration Variables and Clock logic policies
/// </summary>
internal static class AdminManager //stage 4
{
    #region Stage 4-7

    private static readonly DalApi.IDal s_dal = DalApi.Factory.Get; //stage 4

    /// <summary>
    /// Property for providing current application's clock value for any BL class that may need it
    /// </summary>
    internal static DateTime Now => s_dal.Config.Clock; //stage 4

    internal static event Action? ConfigUpdatedObservers; //stage 5 - for config update observers
    internal static event Action? ClockUpdatedObservers; //stage 5 - for clock update observers

    private static Task? _periodicTask = null; //stage 7

    /// <summary>
    /// Method to update application's clock from any BL class as may be required
    /// </summary>
    /// <param name="newClock">updated clock value</param>
    internal static void UpdateClock(DateTime newClock) //stage 4-7
    {
        var oldClock = s_dal.Config.Clock; //stage 4
        s_dal.Config.Clock = newClock; //stage 4

        //Add calls here to any logic method that should be called periodically,
        //after each clock update
        //for example, Periodic students' updates:
        // - Go through all students to update properties that are affected by the clock update
        // - (students become not active after 5 years etc.)

        //TO_DO: //stage 4
        CourierManager.PeriodicCouriersUpdates(oldClock,
            newClock); //stage 4. to be removed in stage 7 and replaced as below
        DeliveryManager.PeriodicDeliveriesUpdates(oldClock,
            newClock); //stage 4. to be removed in stage 7 and replaced as below
        OrderManager.PeriodicOrdersUpdates(oldClock,
            newClock); //stage 4. to be removed in stage 7 and replaced as below
        //...

        //TO_DO: //stage 7
        //if (_periodicTask is null || _periodicTask.IsCompleted) //stage 7
        //    _periodicTask = Task.Run(() => StudentManager.PeriodicStudentsUpdates(oldClock, newClock));
        //...

        //Calling all the observers of clock update
        ClockUpdatedObservers?.Invoke(); //prepared for stage 5
    }

    /// <summary>
    /// Method for providing current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static BO.Config GetConfig() //stage 4
    {
    return new BO.Config
    {
        ManagerId = s_dal.Config.ManagerId,
        Clock = s_dal.Config.Clock,
        RiskRange = s_dal.Config.RiskRange,
        ComponyAddress = s_dal.Config.ComponyAddress,
        ManagerPassword = s_dal.Config.ManagerPassword,
        Latitude = s_dal.Config.Latitude,
        Longitude = s_dal.Config.Longitude,
        MaxDistance = s_dal.Config.MaxDistance,
        AverageCarSpeed = s_dal.Config.AverageCarSpeed,
        AverageBikeSpeedBike = s_dal.Config.AverageBikeSpeedBike,
        AverageBicycleSpeed = s_dal.Config.AverageBicycleSpeed,
        AverageWalkSpeed = s_dal.Config.AverageWalkSpeed,
        MaxDeliveryTime = s_dal.Config.MaxDeliveryTime,
        TimeToBeDispose = s_dal.Config.TimeToBeDispose
    };
    }

    /// <summary>
    /// Method for setting current configuration variables values for any BL class that may need it
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    internal static void SetConfig(BO.Config configuration) //stage 4
    {
        var configChanged = false; // stage 5

        var props = s_dal.Config.GetType()
            .GetProperties()
            .Where(p => p.CanRead && p.CanWrite); // רק פרופרטיז שאפשר גם לקרוא וגם לכתוב

        foreach (var prop in props)
        {
            var oldValue = prop.GetValue(s_dal.Config);
            var newValue = prop.GetValue(configuration);

            if (Equals(oldValue, newValue)) continue;
            prop.SetValue(s_dal.Config, newValue);
            configChanged = true;
        }

        //Calling all the observers of configuration update
        if (configChanged) // stage 5
            ConfigUpdatedObservers?.Invoke(); // stage 5
    }

    internal static void ResetDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            s_dal.ResetDB(); //stage 4
            UpdateClock(Now); //stage 5 - needed since we want the label on Pl to be updated
            SetConfig(GetConfig()); //stage 5 - needed to update PL 
        }
    }

    internal static void InitializeDB() //stage 4-7
    {
        lock (BlMutex) //stage 7
        {
            DalTest.Initialization.Do(); //stage 4
            UpdateClock(Now); //stage 5 - needed since we want the label on Pl to be updated           
            SetConfig(GetConfig()); //stage 5 - needed for update the PL
        }
    }

    #endregion Stage 4-7

    #region Stage 7 base

    /// <summary>    
    /// Mutex to use from BL methods to get mutual exclusion while the simulator is running
    /// </summary>
    internal static readonly object
        BlMutex = new(); // BlMutex = s_dal; // This field is actually the same as s_dal - it is defined for readability of locks

    /// <summary>
    /// The thread of the simulator
    /// </summary>
    private static volatile Thread? s_thread;

    /// <summary>
    /// The Interval for clock updating
    /// in minutes by second (default value is 1, will be set on Start())    
    /// </summary>
    private static int s_interval = 1;

    /// <summary>
    /// The flag that signs whether simulator is running
    /// 
    private static volatile bool s_stop = false;

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    public static void ThrowOnSimulatorIsRunning()
    {
        if (s_thread is not null)
            throw new BO.BlException("Cannot perform the operation since Simulator is running");
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Start(int interval)
    {
        if (s_thread is null)
        {
            s_interval = interval;
            s_stop = false;
            s_thread = new Thread(clockRunner) { Name = "ClockRunner" };
            s_thread.Start();
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7                                                 
    internal static void Stop()
    {
        if (s_thread is not null)
        {
            s_stop = true;
            s_thread.Interrupt(); //awake a sleeping thread
            s_thread.Name = "ClockRunner stopped";
            s_thread = null;
        }
    }

    private static Task? _simulateTask = null;

    private static void clockRunner()
    {
        while (!s_stop)
        {
            UpdateClock(Now.AddMinutes(s_interval));

            //TO_DO: //stage 7
            //Add calls here to any logic simulation that was required in stage 7
            //for example: course registration simulation
            // if (_simulateTask is null || _simulateTask.IsCompleted)//stage 7
            //     _simulateTask = Task.Run(() => StudentManager.SimulateCourseRegistrationAndGrade());

            //etc...

            try
            {
                Thread.Sleep(1000); // 1 second
            }
            catch (ThreadInterruptedException)
            {
            }
        }
    }

    #endregion Stage 7 base
}