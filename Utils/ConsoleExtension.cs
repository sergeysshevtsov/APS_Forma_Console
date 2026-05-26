using APS_Forma_Console.Enums;

namespace APS_Forma_Console.Utils;
internal static class ConsoleExtension
{
    public static void ConsoleWriteLine(string text, ConsoleTextType consoleTextType = ConsoleTextType.None)
    {
        ConsoleColor color = ConsoleColor.White;
        switch (consoleTextType)
        {
            case ConsoleTextType.None:
                break;
            case ConsoleTextType.Error:
                color = ConsoleColor.Red;
                break;
            case ConsoleTextType.Info:
                color = ConsoleColor.Yellow;
                break;
            case ConsoleTextType.Success:
                color = ConsoleColor.Green;
                break;
        }

        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }
}
