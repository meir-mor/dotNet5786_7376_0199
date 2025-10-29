using Dal;
using DalApi;
using DO;
using System.Threading.Channels;

namespace DalTest
{
    internal class Program
    {
        private static IConfig? s_dalConfig = new ConfigImplementation();
        private static ICourier? s_dalCourier = new CourierImplementation();
        private static IDelivery? s_dalDelivery = new DeliveryImplementation();
        private static IOrder? s_dalOrder = new OrderImplementation();

        static void Main(string[] args)
        {
            try
            {
                RunMainMenu();
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

        private static void RunMainMenu()
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

                if (!int.TryParse(Console.ReadLine(), out int menuChoice))
                {
                    Console.WriteLine(@"Invalid input! (קלט לא תקין)");
                    menuChoice = 0; 
                }

                choice = (MainMenuOption)menuChoice;
                Console.Clear();

                switch (choice)
                {
                    case MainMenuOption.CourierMenu:
                        RunEntityMenu("Courier", s_dalCourier);
                        break;
                    case MainMenuOption.DeliveryMenu:
                        RunEntityMenu("Delivery", s_dalDelivery);
                        break;
                    case MainMenuOption.OrderMenu:
                        RunEntityMenu("Order", s_dalOrder);
                        break;
                    case MainMenuOption.ConfigMenu:
                        RunConfigMenu();
                        break;
                    case MainMenuOption.Initialization:
                        Initialization.Do(s_dalConfig, s_dalOrder, s_dalCourier, s_dalDelivery);
                        Console.WriteLine(@"Data reset and initialized successfully! (אתחול)");
                        break;
                    case MainMenuOption.ShowAllData:
                        ShowAllData();
                        break;
                    case MainMenuOption.ResetAll:
                        ResetAll();
                        break;
                    case MainMenuOption.Exit:
                        Console.WriteLine(@"Exiting... (יציאה)");
                        break;
                    default:
                        Console.WriteLine(@"Invalid choice! (בחירה לא חוקית)");
                        break;
                }

                if (choice == MainMenuOption.Exit) continue;
                Console.WriteLine(@"\nPress any key to return to main menu... (חזרה לתפריט)");
                Console.ReadKey();

            } while (choice != MainMenuOption.Exit);
        }

        private static void RunEntityMenu(string entityName, dynamic? dalEntity)
        {
            if (dalEntity == null)
            {
                Console.WriteLine($@" No implementation for {entityName} (אין יישום)");
                return;
            }

            EntityMenuOption choice;
            do
            {
                Console.Clear();
                Console.WriteLine($@"====== {entityName} Menu (תפריט {entityName}) ======");
                Console.WriteLine(@"1. Add new entity (הוסף חדש)");
                Console.WriteLine(@"2. Show by ID (הצג לפי מזהה)");
                Console.WriteLine(@"3. Show all entities (הצג הכל)");
                Console.WriteLine(@"4. Update existing entity (עדכון)");
                Console.WriteLine(@"5. Delete by ID (מחק לפי מזהה)");
                Console.WriteLine(@"6. Delete all entities (מחק הכל)");
                Console.WriteLine(@"0. Return to main menu (חזרה)");
                Console.Write(@"Select option: (בחר אפשרות) ");

                if (!int.TryParse(Console.ReadLine(),out int menuChoice))
                {
                Console.WriteLine("Invalid input! ");
                choice = 0;
                }

                choice = (EntityMenuOption)menuChoice; //(EntityMenuOption)int.Parse(Console.ReadLine() ?? "0");
                Console.Clear();

                try
                {
                    switch (choice)
                    {
                        case EntityMenuOption.Create:
                            CreateEntity(dalEntity, entityName);
                            break;
                        case EntityMenuOption.Read:
                            ReadEntity(dalEntity);
                            break;
                        case EntityMenuOption.ReadAll:
                            ReadAllEntities(dalEntity);
                            break;
                        case EntityMenuOption.Update:
                            Console.WriteLine(@"Update not implemented yet. (עדכון טרם מומש)");
                            break;
                        case EntityMenuOption.Delete:
                            DeleteEntity(dalEntity);
                            break;
                        case EntityMenuOption.DeleteAll:
                            dalEntity.DeleteAll();
                            Console.WriteLine(@" All data deleted. (נמחק הכל)");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($@" Error: {ex.Message} (שגיאה)");
                }

                if (choice == EntityMenuOption.Exit) continue;
                Console.WriteLine(@"\nPress any key to continue... (המשך)");
                Console.ReadKey();

            } while (choice != EntityMenuOption.Exit);
        }

        //CONFIG MENU
        private static void RunConfigMenu()
        {
            ConfigMenuOption choice;
            do
            {
                Console.Clear();
                Console.WriteLine(@"====== Configuration Menu (תפריט תצורה) ======");
                Console.WriteLine(@"1. Advance clock by 1 minute (קדם דקה)");
                Console.WriteLine(@"2. Advance clock by 1 hour (קדם שעה)");
                Console.WriteLine(@"3. Show current clock (הצג שעה)");
                Console.WriteLine(@"4. Set new clock (קבע שעה חדשה)");
                Console.WriteLine(@"5. Reset configuration (איפוס)");
                Console.WriteLine(@"0. Return to main menu (חזרה)");
                Console.Write(@"Select option: (בחר אפשרות) ");

                if(!int.TryParse(Console.ReadLine(), out int menuChoice))
                {
                    Console.WriteLine("Invalid input! ");
                    menuChoice = 0;
                }
                choice = (ConfigMenuOption)menuChoice; //(ConfigMenuOption)int.Parse(Console.ReadLine() ?? "0");
                Console.Clear();

                switch (choice)
                {
                    case ConfigMenuOption.AdvanceMinute:
                        if (s_dalConfig?.Clock == null) break;
                        s_dalConfig.Clock = s_dalConfig.Clock.AddMinutes(1);
                        Console.WriteLine(@"Clock advanced by one minute. (קודם בדקה)");
                        break;
                    case ConfigMenuOption.AdvanceHour:
                        if (s_dalConfig?.Clock == null) break;
                        s_dalConfig.Clock = s_dalConfig.Clock.AddHours(1);
                        Console.WriteLine(@"Clock advanced by one hour. (קודם בשעה)");
                        break;
                    case ConfigMenuOption.ShowClock:
                        Console.WriteLine($@"Current time: {s_dalConfig?.Clock} (שעה נוכחית)");
                        break;
                    case ConfigMenuOption.SetClock:
                        Console.Write(@"Enter new time (yyyy-MM-dd HH:mm): (הכנס שעה חדשה) ");
                        if (DateTime.TryParse(Console.ReadLine(), out var newTime))
                        {
                            if (s_dalConfig?.Clock == null) break;
                            s_dalConfig.Clock = newTime;
                            Console.WriteLine(@"Clock updated. (עודכן)");
                        }
                        else
                        {
                            Console.WriteLine(@"Invalid format. (פורמט לא תקין)");
                        }
                        break;
                    case ConfigMenuOption.Reset:
                        s_dalConfig?.Reset();
                        Console.WriteLine(@"Configuration reset. (אופסה)");
                        break;
                }

                if (choice != ConfigMenuOption.Exit)
                {
                    Console.WriteLine(@"\nPress any key to continue... (המשך)");
                    Console.ReadKey();
                }

            } while (choice != ConfigMenuOption.Exit);
        }

        //HELPERS
        private static void CreateEntity(dynamic dalEntity, string entityName)
        {
            Console.WriteLine($@"Add new {entityName}: (הוסף חדש)");
            Console.Write(@"Enter ID: (מזהה) ");
            //var id = int.Parse(Console.ReadLine() ?? "0");
            if (!int.TryParse(Console.ReadLine(), out int id))
            { Console.WriteLine("Invalid ID! ");
                return;
            }
            var newEntity = Activator.CreateInstance(dalEntity.GetType().GenericTypeArguments[0], id);

            dalEntity.Create(newEntity);
            Console.WriteLine($@" {entityName} added successfully. (נוסף)");
        }

        private static void ReadEntity(dynamic dalEntity)
        {
            Console.Write(@"Enter ID to search: (הכנס מזהה) ");
            //var id = int.Parse(Console.ReadLine() ?? "0");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID! ");
                return;
            }
            var entity = dalEntity.Read(id);
            if (entity == null)
            {
                Console.WriteLine(@" Not found. (לא נמצא)");
                return;
            }
            Console.WriteLine(entity);
        }

        private static void ReadAllEntities(dynamic dalEntity)
        {
            var list = dalEntity.ReadAll();
            foreach (var item in list)
                Console.WriteLine(item);
        }

        private static void DeleteEntity(dynamic dalEntity)
        {
            Console.Write(@"Enter ID to delete: (הכנס מזהה) ");
            //int id = int.Parse(Console.ReadLine() ?? "0");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid ID! ");
                return;
            }
            dalEntity.Delete(id);
            Console.WriteLine(@"Entity deleted successfully. (נמחק)");
        }

        private static void ShowAllData()
        {
            Console.WriteLine(@"=== All Data (כל הנתונים) ===");
            Console.WriteLine(@"Couriers: (שליחים)");
            ReadAllEntities(s_dalCourier);
            Console.WriteLine(@"\nDeliveries: (משלוחים)");
            ReadAllEntities(s_dalDelivery);
            Console.WriteLine(@"\nOrders: (הזמנות)");
            ReadAllEntities(s_dalOrder);
        }

        private static void ResetAll()
        {
            s_dalCourier?.DeleteAll();
            s_dalDelivery?.DeleteAll();
            s_dalOrder?.DeleteAll();
            s_dalConfig?.Reset();
            Console.WriteLine(@"All data reset successfully. (אופס)");
        }
    }
}

