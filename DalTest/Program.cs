using Dal;
using DalApi;
using DalTest.Menu;
namespace DalTest;
internal class Program
{
    //internal static IConfig? s_dalConfig = new ConfigImplementation();
    //internal static ICourier? s_dalCourier = new CourierImplementation();
    //internal static IDelivery? s_dalDelivery = new DeliveryImplementation();
    //internal static IOrder? s_dalOrder = new OrderImplementation();

    static readonly IDal s_dal = new DalList();
    private static void Main(string[] args)
    {
        try
        {
            MainMenu.RunMainMenu();
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"Error: {ex.Message} (שגיאה)");
            if (ex.InnerException != null)
                Console.WriteLine($@"Inner error: {ex.InnerException.Message} (שגיאה פנימית)");
        }
        finally
        {
            Console.WriteLine(@"\nPress any key to exit... (יציאה)");
            Console.ReadKey();
        }
    }
}