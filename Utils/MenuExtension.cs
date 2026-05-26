using System.Runtime.CompilerServices;

namespace APS_Forma_Console.Utils;
internal class MenuExtension
{
    public static void MenuHeader(string text)
    {
        ConsoleExtension.ConsoleWriteLine(new string('=', text.Length));
        ConsoleExtension.ConsoleWriteLine(text);
        ConsoleExtension.ConsoleWriteLine(new string('=', text.Length));
    }

    public static int ReadSelection(int min, int max)
    {
        while (true)
        {
            Console.Write("Select option: ");
            string? input = Console.ReadLine();
            Console.WriteLine();

            if (int.TryParse(input, out var value) && value >= min && value <= max)
                return value;

            Console.WriteLine($"Enter a valid item number between {min} and {max}.");
        }
    }

    public static int MenuItemsRender(List<string> items, bool showBack = false, bool showExit = false)
    {
        if (showBack) items.Add("Back");
        for (int i = 0; i < items.Count; i++)
            Console.WriteLine($"{i + 1}. {items[i]}");
        if (showBack)
            Console.WriteLine("0. Exit");

        return ReadSelection(0, items.Count);
    }
}
