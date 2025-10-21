using System;

partial class Program7376
{
    static void Main()
    {
        Welcome7376();
        Welcome0199();
        Console.ReadKey();
        //nothing at all
    }

    static partial void Welcome0199();
    private static void Welcome7376()
    {
        Console.Write("Enter your name: ");
        string name = Console.ReadLine();

        Console.WriteLine("{0}, Welcome to my first console application", name);
    }
}
