using DalApi;
using DalTest.Menu;
namespace Dal;

internal class Program
{
    static readonly IDal s_dal = new DalList();
    private static void Main(string[] args)
    {
        try
        {
            MainMenu.RunMainMenu(s_dal);
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