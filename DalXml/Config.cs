using System.Runtime.CompilerServices;

namespace Dal;

internal static class Config
{
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_couriers_xml = "couriers.xml";
    internal const string s_orders_xml = "orders.xml";
    internal const string s_deliveries_xml = "deliveries.xml";

    internal static int NextOrderId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextOrderId");
        [MethodImpl(MethodImplOptions.Synchronized)]
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextOrderId", value);
    }

    internal static int NextDeliveryId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextDeliveryId");
        [MethodImpl(MethodImplOptions.Synchronized)]
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextDeliveryId", value);
    }

    internal static DateTime Clock
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    internal static int ManagerId
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigIntVal(s_data_config_xml, "ManagerId");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigIntVal(s_data_config_xml, "ManagerId", value);
    }

    internal static string ManagerPassword
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigStringVal(s_data_config_xml, "ManagerPassword");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigStringVal(s_data_config_xml, "ManagerPassword", value);
    }

    internal static string? ComponyAddress
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigStringVal(s_data_config_xml, "ComponyAddress");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigStringVal(s_data_config_xml, "ComponyAddress", value ?? "");
    }

    internal static double? Latitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "Latitude");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "Latitude", value ?? 0);
    }

    internal static double? Longitude
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "Longitude");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "Longitude", value ?? 0);
    }

    internal static double? MaxDistance
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "MaxDistance");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "MaxDistance", value ?? 0);
    }

    internal static double AverageCarSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageCarSpeed");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageCarSpeed", value);
    }

    internal static double AverageBikeSpeedBike
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageBikeSpeedBike");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageBikeSpeedBike", value);
    }

    internal static double AverageBicycleSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageBicycleSpeed");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageBicycleSpeed", value);
    }

    internal static double AverageWalkSpeed
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageWalkSpeed");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageWalkSpeed", value);
    }

    internal static TimeSpan RiskRange
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "RiskRange");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "RiskRange", value);
    }

    internal static TimeSpan MaxDeliveryTime
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "MaxDeliveryTime");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "MaxDeliveryTime", value);
    }

    internal static TimeSpan TimeToBeDispose
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "TimeToBeDispose");
        [MethodImpl(MethodImplOptions.Synchronized)]
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "TimeToBeDispose", value);
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    internal static void Reset()
    {
        NextDeliveryId = 0;
        NextOrderId = 0;
        Clock = DateTime.Now;
        ManagerId = 1234;
        ManagerPassword = "1234";
        ComponyAddress = "Bar Cochva 2 BneiBrak Israel";
        Latitude = 32.09376490211114;
        Longitude = 34.82291411745883;
        MaxDistance = 100;
        AverageCarSpeed = 60;
        AverageBikeSpeedBike = 20;
        AverageBicycleSpeed = 15;
        AverageWalkSpeed = 5;
        MaxDeliveryTime = TimeSpan.FromHours(10);
        TimeToBeDispose = TimeSpan.FromDays(365);
        RiskRange = TimeSpan.FromMinutes(30);
    }
}