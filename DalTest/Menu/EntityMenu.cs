using DalApi;
using DO;
using static DalTest.Helpers.EntityActions;

namespace DalTest.Menu;

internal static class EntityMenu
{
    public static void RunEntityMenu<T>(string entityName, ICrud<T>? dalEntity, IDal dal) where T : Entity
    {
        if (dalEntity is null)
        {
            Console.WriteLine($@"No implementation for {entityName} (אין יישום)");
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
            Console.WriteLine(@"0. Return (חזרה)");
            Console.Write(@"Select option: (בחר אפשרות) ");

            if (!int.TryParse(Console.ReadLine(), out var menuChoice))
                menuChoice = 0;

            choice = (EntityMenuOption)menuChoice;
            Console.Clear();

            try
            {
                switch (choice)
                {
                    case EntityMenuOption.Create:
                        CreateEntity(dalEntity, entityName, dal);
                        break;
                    case EntityMenuOption.Read:
                        ReadEntity(dalEntity);
                        break;
                    case EntityMenuOption.ReadAll:
                        ReadAllEntities(dalEntity);
                        break;
                    case EntityMenuOption.Update:
                        UpdateEntity(dalEntity, entityName, dal);
                        break;
                    case EntityMenuOption.Delete:
                        DeleteEntity(dalEntity);
                        break;
                    case EntityMenuOption.DeleteAll:
                        dalEntity.DeleteAll();
                        Console.WriteLine(@"All data deleted.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Error: {ex.Message}");
            }

            if (choice == EntityMenuOption.Exit) continue;
            Console.WriteLine(@"\nPress any key to continue...");
            Console.ReadKey();
        } while (choice != EntityMenuOption.Exit);
    }
}