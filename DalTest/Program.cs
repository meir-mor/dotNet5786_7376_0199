using Dal;
using DalApi;
using DO;

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
                Console.WriteLine($"שגיאה: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"שגיאה פנימית: {ex.InnerException.Message}");
            }
            finally
            {
                Console.WriteLine("\nלחץ על מקש כלשהו כדי לצאת...");
                Console.ReadKey();
            }
        }

        private static void RunMainMenu()
        {
            MainMenuOption choice;

            do
            {
                Console.Clear();
                Console.WriteLine("====== תפריט ראשי ======");
                Console.WriteLine("1. ניהול שליחים");
                Console.WriteLine("2. ניהול משלוחים");
                Console.WriteLine("3. ניהול הזמנות");
                Console.WriteLine("4. ניהול תצורה (Config)");
                Console.WriteLine("5. אתחול נתונים");
                Console.WriteLine("6. הצגת כל הנתונים");
                Console.WriteLine("7. איפוס כל הנתונים");
                Console.WriteLine("0. יציאה");
                Console.Write("בחר אפשרות: ");

                choice = (MainMenuOption)int.Parse(Console.ReadLine() ?? "0");
                Console.Clear();

                switch (choice)
                {
                    case MainMenuOption.CourierMenu:
                        RunEntityMenu("שליח", s_dalCourier);
                        break;
                    case MainMenuOption.DeliveryMenu:
                        RunEntityMenu("משלוח", s_dalDelivery);
                        break;
                    case MainMenuOption.OrderMenu:
                        RunEntityMenu("הזמנה", s_dalOrder);
                        break;
                    case MainMenuOption.ConfigMenu:
                        RunConfigMenu();
                        break;
                    case MainMenuOption.Initialization:
                        Initialization.Do(s_dalConfig, s_dalOrder, s_dalCourier, s_dalDelivery);
                        Console.WriteLine("✅ הנתונים אופסו ואוכלסו בהצלחה!");
                        break;
                    case MainMenuOption.ShowAllData:
                        ShowAllData();
                        break;
                    case MainMenuOption.ResetAll:
                        ResetAll();
                        break;
                    case MainMenuOption.Exit:
                        Console.WriteLine("👋 יציאה...");
                        break;
                    default:
                        Console.WriteLine("בחירה לא חוקית!");
                        break;
                }

                if (choice == MainMenuOption.Exit) continue;
                Console.WriteLine("\nלחץ על מקש כדי לחזור לתפריט הראשי...");
                Console.ReadKey();

            } while (choice != MainMenuOption.Exit);
        }

        private static void RunEntityMenu(string entityName, dynamic? dalEntity)
        {
            if (dalEntity == null)
            {
                Console.WriteLine($"❌ אין יישום עבור {entityName}");
                return;
            }

            EntityMenuOption choice;
            do
            {
                Console.Clear();
                Console.WriteLine($"====== תפריט {entityName} ======");
                Console.WriteLine("1. הוסף אובייקט חדש");
                Console.WriteLine("2. הצג לפי מזהה");
                Console.WriteLine("3. הצג את כל האובייקטים");
                Console.WriteLine("4. עדכן אובייקט קיים");
                Console.WriteLine("5. מחק אובייקט לפי מזהה");
                Console.WriteLine("6. מחק את כל האובייקטים");
                Console.WriteLine("0. חזרה לתפריט הראשי");
                Console.Write("בחר אפשרות: ");

                choice = (EntityMenuOption)int.Parse(Console.ReadLine() ?? "0");
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
                            Console.WriteLine("עדכון אובייקט — טרם מומש.");
                            break;
                        case EntityMenuOption.Delete:
                            DeleteEntity(dalEntity);
                            break;
                        case EntityMenuOption.DeleteAll:
                            dalEntity.DeleteAll();
                            Console.WriteLine("✅ כל הנתונים נמחקו.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ שגיאה: {ex.Message}");
                }

                if (choice == EntityMenuOption.Exit) continue;
                Console.WriteLine("\nלחץ על מקש כדי להמשיך...");
                Console.ReadKey();

            } while (choice != EntityMenuOption.Exit);
        }

        // --- CONFIG MENU ---
        private static void RunConfigMenu()
        {
            ConfigMenuOption choice;
            do
            {
                Console.Clear();
                Console.WriteLine("====== תפריט תצורה ======");
                Console.WriteLine("1. קדם שעון בדקה");
                Console.WriteLine("2. קדם שעון בשעה");
                Console.WriteLine("3. הצג שעון נוכחי");
                Console.WriteLine("4. קבע שעון חדש");
                Console.WriteLine("5. אפס תצורה");
                Console.WriteLine("0. חזרה לתפריט הראשי");
                Console.Write("בחר אפשרות: ");

                choice = (ConfigMenuOption)int.Parse(Console.ReadLine() ?? "0");
                Console.Clear();

                switch (choice)
                {
                    case ConfigMenuOption.AdvanceMinute:
                        if(s_dalConfig?.Clock == null) break;
                        s_dalConfig.Clock = s_dalConfig.Clock.AddMinutes(1);
                        Console.WriteLine("✅ השעון קודם בדקה.");
                        break;
                    case ConfigMenuOption.AdvanceHour:
                        if(s_dalConfig?.Clock == null) break;
                        s_dalConfig.Clock = s_dalConfig.Clock.AddHours(1);
                        break;
                    case ConfigMenuOption.ShowClock:
                        Console.WriteLine($"🕒 השעה הנוכחית: {s_dalConfig?.Clock}");
                        break;
                    case ConfigMenuOption.SetClock:
                        Console.Write("הכנס שעה חדשה (yyyy-MM-dd HH:mm): ");
                        if (DateTime.TryParse(Console.ReadLine(), out var newTime))
                        {
                            if(s_dalConfig?.Clock == null) break;
                            s_dalConfig.Clock = newTime;
                            Console.WriteLine("✅ השעה עודכנה.");
                        }
                        else
                        {
                            Console.WriteLine("❌ פורמט לא תקין.");
                        }
                        break;
                    case ConfigMenuOption.Reset:
                        s_dalConfig?.Reset();
                        Console.WriteLine("✅ התצורה אופסה.");
                        break;
                }

                if (choice != ConfigMenuOption.Exit)
                {
                    Console.WriteLine("\nלחץ על מקש כדי להמשיך...");
                    Console.ReadKey();
                }

            } while (choice != ConfigMenuOption.Exit);
        }

        // --- HELPERS ---
        private static void CreateEntity(dynamic dalEntity, string entityName)
        {
            Console.WriteLine($"הוספת {entityName} חדש:");
            Console.Write("הכנס מזהה: ");
            var id = int.Parse(Console.ReadLine() ?? "0");

            // כאן תוכל להוסיף קליטת נתונים נוספים לפי סוג הישות
            var newEntity = Activator.CreateInstance(dalEntity.GetType().GenericTypeArguments[0], id);

            dalEntity.Create(newEntity);
            Console.WriteLine($"✅ {entityName} נוסף בהצלחה.");
        }

        private static void ReadEntity(dynamic dalEntity)
        {
            Console.Write("הכנס מזהה לחיפוש: ");
            var id = int.Parse(Console.ReadLine() ?? "0");
            var entity = dalEntity.Read(id);
            Console.WriteLine(entity != null ? entity.ToString() : "❌ לא נמצא.");
        }

        private static void ReadAllEntities(dynamic dalEntity)
        {
            var list = dalEntity.ReadAll();
            foreach (var item in list)
                Console.WriteLine(item);
        }

        private static void DeleteEntity(dynamic dalEntity)
        {
            Console.Write("הכנס מזהה למחיקה: ");
            int id = int.Parse(Console.ReadLine() ?? "0");
            dalEntity.Delete(id);
            Console.WriteLine("✅ האובייקט נמחק בהצלחה.");
        }

        private static void ShowAllData()
        {
            Console.WriteLine("=== רשימת כל הנתונים ===");
            Console.WriteLine("שליחים:");
            ReadAllEntities(s_dalCourier);
            Console.WriteLine("\nמשלוחים:");
            ReadAllEntities(s_dalDelivery);
            Console.WriteLine("\nהזמנות:");
            ReadAllEntities(s_dalOrder);
        }

        private static void ResetAll()
        {
            s_dalCourier?.DeleteAll();
            s_dalDelivery?.DeleteAll();
            s_dalOrder?.DeleteAll();
            s_dalConfig?.Reset();
            Console.WriteLine("✅ כל הנתונים אופסו בהצלחה.");
        }
    }
}
