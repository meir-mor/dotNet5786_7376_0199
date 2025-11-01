using DO;
using static DalTest.Program;

namespace DalTest.Menu;

internal static class ConfigMenu
{
    public static void RunConfigMenu()
    {
        ConfigMenuOption choice;
        do
        {
            Console.Clear();
            Console.WriteLine(@"====== Configuration Menu ======");
            Console.WriteLine(@"1. Advance clock by 1 minute");
            Console.WriteLine(@"2. Advance clock by 1 hour");
            Console.WriteLine(@"3. Show current clock");
            Console.WriteLine(@"4. Set new clock");
            Console.WriteLine(@"5. Reset configuration");
            Console.WriteLine(@"0. Return");
            Console.Write(@"Select option: ");

            if (!int.TryParse(Console.ReadLine(), out int menuChoice))
                menuChoice = 0;

            choice = (ConfigMenuOption)menuChoice;
            Console.Clear();

            switch (choice)
            {
                case ConfigMenuOption.AdvanceMinute:
                    s_dalConfig!.Clock = s_dalConfig.Clock.AddMinutes(1);
                    Console.WriteLine(@"Clock advanced by one minute.");
                    break;
                case ConfigMenuOption.AdvanceHour:
                    s_dalConfig!.Clock = s_dalConfig.Clock.AddHours(1);
                    Console.WriteLine(@"Clock advanced by one hour.");
                    break;
                case ConfigMenuOption.ShowClock:
                    Console.WriteLine($@"Current time: {s_dalConfig?.Clock}");
                    break;
                case ConfigMenuOption.SetClock:
                    Console.Write(@"Enter new time (yyyy-MM-dd HH:mm): ");
                    if (DateTime.TryParse(Console.ReadLine(), out var newTime))
                    {
                        s_dalConfig!.Clock = newTime;
                        Console.WriteLine(@"Clock updated.");
                    }
                    else
                    {
                        Console.WriteLine(@"Invalid format.");
                    }
                    break;
                case ConfigMenuOption.Reset:
                    s_dalConfig?.Reset();
                    Console.WriteLine(@"Configuration reset.");
                    break;
            }

            if (choice != ConfigMenuOption.Exit) continue;
            Console.WriteLine(@"\nPress any key to continue...");
            Console.ReadKey();

        } while (choice != ConfigMenuOption.Exit);
    }
}
