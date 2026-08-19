using System;
using System.Threading;

namespace CyberHeistButuan.UI
{
    public static class Terminal_Render
    {
        public static void TypewriterPrint(string text, int delay = 15)
        {
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(delay);
            }
            Console.WriteLine();
        }

        public static void PrintAmbient(string text, bool typewriter = false)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            if (typewriter) TypewriterPrint(text, 15);
            else Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void PrintRoll(string text, bool typewriter = false)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            if (typewriter) TypewriterPrint(text, 15);
            else Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void PrintAlarm(string text, bool typewriter = false)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            if (typewriter) TypewriterPrint(text, 15);
            else Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void PrintSuccess(string text, bool typewriter = false)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            if (typewriter) TypewriterPrint(text, 15);
            else Console.WriteLine(text);
            Console.ResetColor();
        }

        public static void PrintStatusWarning(string title, string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n================================================================================================");
            Console.WriteLine($"\t[ALERT]  {title.ToUpper()}  [ALERT]");
            Console.WriteLine("==================================================================================================");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("==================================================================================================");
            Console.ResetColor();
        }
    }
}