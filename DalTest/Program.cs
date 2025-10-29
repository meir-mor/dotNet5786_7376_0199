
using Dal;
using DalApi;

namespace DalTest
{
    internal class Program
    {
        public static IDelivery? s_dalDelivery=new DeliveryImplementation();
        private static IOrder? s_dalCourse = new OrderImplementation(); 
        private static ICourier? s_dalLink = new CourierImplementation();
        private static IConfig? s_dalConfig = new ConfigImplementation(); 
        static void Main(string[] args)
        {
            try
            {
                MainMenu();
            }
            catch
            {
                throw new NotImplementedException();
            }
        }
        private static void MainMenu()
        {
        }
    }
}
