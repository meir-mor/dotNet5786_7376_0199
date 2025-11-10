namespace Dal;

internal static class Config
{
    internal const string s_data_config_xml = "data-config.xml";
    internal const string s_couriers_xml = "couriers.xml";
    internal const string s_orders_xml = "orders.xml";
    internal const string s_deliveries_xml = "deliveries.xml";

    internal static int NextOrderId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextOrderId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextOrderId", value);
    }

    internal static int NextDeliveryId
    {
        get => XMLTools.GetAndIncreaseConfigIntVal(s_data_config_xml, "NextDeliveryId");
        private set => XMLTools.SetConfigIntVal(s_data_config_xml, "NextDeliveryId", value);
    }

    internal static DateTime Clock
    {
        get => XMLTools.GetConfigDateVal(s_data_config_xml, "Clock");
        set => XMLTools.SetConfigDateVal(s_data_config_xml, "Clock", value);
    }

    internal static int ManagerId
    {
        get => XMLTools.GetConfigIntVal(s_data_config_xml, "ManagerId");
        set => XMLTools.SetConfigIntVal(s_data_config_xml, "ManagerId", value);
    }

    internal static string ManagerPassword 
    {
        get => XMLTools.GetConfigStringVal(s_data_config_xml, "ManagerPassword");
        set => XMLTools.SetConfigStringVal(s_data_config_xml, "ManagerPassword", value);
    }

    internal static string ComponyAddress 
    {
        get => XMLTools.GetConfigStringVal(s_data_config_xml, "ComponyAddress");
        set => XMLTools.SetConfigStringVal(s_data_config_xml, "ComponyAddress", value);
    }
    internal static double Latitude 
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "Latitude");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "Latitude", value);
    }

    internal static double Longitude
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "Longitude");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "Longitude", value);
    }
    internal static double? MaxDistance
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "MaxDistance");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "MaxDistance", value ?? 0);
    }
    internal static double AverageCarSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageCarSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageCarSpeed", value);
    }
    internal static double AverageBikeSpeedBike
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageBikeSpeedBike");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageBikeSpeedBike", value);
    }
    internal static double AverageBicycleSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageBicycleSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageBicycleSpeed", value);
    }
    internal static double AverageWalkSpeed
    {
        get => XMLTools.GetConfigDoubleVal(s_data_config_xml, "AverageWalkSpeed");
        set => XMLTools.SetConfigDoubleVal(s_data_config_xml, "AverageWalkSpeed", value);
    }
    internal static TimeSpan RiskRange
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "RiskRange");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "RiskRange", value);
    }
    internal static TimeSpan MaxDeliveryTime
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "MaxDeliveryTime");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "MaxDeliveryTime", value);
    }
    internal static TimeSpan TimeToBeDispose
    {
        get => XMLTools.GetConfigTimeSpanVal(s_data_config_xml, "TimeToBeDispose");
        set => XMLTools.SetConfigTimeSpanVal(s_data_config_xml, "TimeToBeDispose", value);
    }
    internal static void Reset()
    {
        NextOrderId = 0;
        NextDeliveryId = 0;
        Clock = DateTime.Now;
        RiskRange = TimeSpan.FromMinutes(30);

        ManagerId = 0;
        ComponyAddress = "Bar Cochva 2 BneiBrak Israel";
    }



}
