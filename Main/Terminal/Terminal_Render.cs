using System;
using System.Threading;

namespace CyberHeistAce.UI
{
    public static class TerminalRenderer
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

        /// <summary>
        /// Prints monitored/unmonitored labels consistently using Yellow text.
        /// </summary>
        public static void PrintSecurityStatus(string label, bool isMonitored)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[{label}: {(isMonitored ? "MONITORED" : "UNMONITORED")}]");
            Console.ResetColor();
        }
    }
}