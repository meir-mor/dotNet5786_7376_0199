using Dal;
using DalApi;
using DalTest.Helpers;
using dalTest.Menu;
using DO;

namespace DalTest.Menu;

internal static class MainMenu
{
    public static void RunMainMenu(IDal dal)
    {
        MainMenuOption choice;
        do
        {
            Console.Clear();
            Console.WriteLine(@"====== Main Menu (תפריט ראשי) ======");
            Console.WriteLine(@"1. Manage Couriers (ניהול שליחים)");
            Console.WriteLine(@"2. Manage Deliveries (ניהול משלוחים)");
            Console.WriteLine(@"3. Manage Orders (ניהול הזמנות)");
            Console.WriteLine(@"4. Configuration (תצורה)");
            Console.WriteLine(@"5. Initialize Data (אתחול נתונים)");
            Console.WriteLine(@"6. Show All Data (הצגת נתונים)");
            Console.WriteLine(@"7. Reset All Data (איפוס נתונים)");
            Console.WriteLine(@"0. Exit (יציאה)");
            Console.Write(@"Select option: (בחר אפשרות) ");

            if (!int.TryParse(Console.ReadLine(), out var menuChoice))
                menuChoice = 0;

            choice = (MainMenuOption)menuChoice;
            Console.Clear();

            switch (choice)
            {
                case MainMenuOption.CourierMenu:
                    EntityMenu.RunEntityMenu("Courier", dal.Courier, dal);
                    break;
                case MainMenuOption.DeliveryMenu:
                    EntityMenu.RunEntityMenu("Delivery", dal.Delivery, dal);
                    break;
                case MainMenuOption.OrderMenu:
                    EntityMenu.RunEntityMenu("Order", dal.Order, dal);
                    break;
                case MainMenuOption.ConfigMenu:
                    ConfigMenu.RunConfigMenu(dal);
                    break;
                case MainMenuOption.Initialization:
                    Initialization.Do(dal);
                    Console.WriteLine(@"Data reset and initialized successfully! (אתחול)");
                    break;
                case MainMenuOption.ShowAllData:
                    EntityActions.ShowAllData(dal);
                    break;
                case MainMenuOption.ResetAll:
                    dal.ResetDB();
                    break;
            }

            if (choice == MainMenuOption.Exit) continue;
            Console.WriteLine(@"\nPress any key to return... (חזרה)");
            Console.ReadKey();
        } while (choice != MainMenuOption.Exit);
    }
}