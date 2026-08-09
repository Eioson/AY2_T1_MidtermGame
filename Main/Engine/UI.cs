using System;
using System.Threading;
using Main.Models;

namespace Main.Engine
{
    public static class UI
    {
        // Typewriter effect printing text character by character
        public static void TypewriterPrint(string text, int delayMilliseconds = 25, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(delayMilliseconds);
            }
            Console.WriteLine();
            Console.ResetColor();
        }

        // ASCII Game Header Banner
        public static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(@"
╔══════════════════════════════════════════════════════════════════╗
║                   C Y B E R - H E I S T   2 0 8 8                ║
║                 [ ACE Hospital Corporate Mainframe ]            ║
╚══════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        // Display current Detection State with color-coding
        public static void DisplayStatusHeader(Player player, string roomName)
        {
            Console.Clear();
            PrintHeader();

            Console.Write(" [LOCATION]: ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{roomName.ToUpper()}\n");
            Console.ResetColor();

            Console.Write(" [PLAYER]: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{player.Name} | HP: {player.CurrentHP}/{player.MaxHP}");
            Console.ResetColor();

            Console.Write(" | [SECURITY STATUS]: ");
            
            // Set security status color based on DetectionState
            switch (player.CurrentState)
            {
                case DetectionState.Undetected:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("UNDETECTED (GREEN ZONE)");
                    break;
                case DetectionState.Suspicious:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("SUSPICIOUS (YELLOW ALERT)");
                    break;
                case DetectionState.Detected:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("DETECTED (ELEVATED ALARM)");
                    break;
                case DetectionState.In_Encounter:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("IN COMBAT (RED ALERT)");
                    break;
            }
            Console.ResetColor();
            Console.WriteLine(new string('─', 68));
        }

        // Styled Dice Roll Result Notification
        public static void DisplayDiceRoll(string statName, int roll, int modifier, int total, int dc, bool success, bool isNat20, bool isNat1)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            TypewriterPrint($"[D20 CHECK] Rolling for {statName.ToUpper()}...", 15, ConsoleColor.Yellow);
            Console.WriteLine($" 🎲 Base Roll: {roll} | Modifier (+{modifier}) = Total: {total} (Target DC: {dc})");

            if (isNat20)
            {
                TypewriterPrint(" ★ CRITICAL SUCCESS! (NATURAL 20) ★", 20, ConsoleColor.Green);
            }
            else if (isNat1)
            {
                TypewriterPrint(" ⚠ CRITICAL FAILURE! (NATURAL 1) ⚠", 20, ConsoleColor.Red);
            }
            else if (success)
            {
                TypewriterPrint(" ✓ CHECK PASSED!", 15, ConsoleColor.Green);
            }
            else
            {
                TypewriterPrint(" ✗ CHECK FAILED!", 15, ConsoleColor.Red);
            }
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}