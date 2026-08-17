using System;
using System.Collections.Generic;
using CyberHeistAce.Models;

namespace CyberHeistAce.UI
{
    public static class AsciiArt
    {
        public static void DisplayTitleBanner()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(@"
================================================================================
      ______      __                 __  __     _      __     ___                
     / ____/_  __/ /_  ___  _____   / / / /__  (_)____/ /_   /   | ________      
    / /   / / / / __ \/ _ \/ ___/  / /_/ / _ \/ / ___/ __/  / /| |/ ___/ _ \     
   / /___/ /_/ / /_/ /  __/ /     / __  /  __/ (__  ) /_   / ___ / /__/  __/     
   \____/\__, /_.___/\___/_/     /_/ /_/\___/_/____/\__/  /_/  |_\___/\___/      
        /____/                                                                   
================================================================================
                    -= [ WELCOME TO CYBER HEIST ACE ] =-
================================================================================");
            Console.ResetColor();
        }

        public static void DisplayHospitalMap()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(@"
================================================================================
ACE HOSPITAL - FLOOR 3 [SECURITY MAP]
    [Lobby] ---> [Corridor] ---> [Server Mainframe] (GOAL)
                     |
                [Vent Shaft]
================================================================================");
            Console.ResetColor();
        }

        /// <summary>
        /// Renders a dynamic route helper pointing to the current location and listing the adjacent room choices.
        /// </summary>
        public static void RenderAdaptiveMap(RoomNode currentRoom, List<RoomNode> availableChoices)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("\n======================= ACCESS ROUTE HELPER =======================");
            Console.Write("  [CURRENT LOCATION] : ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[YOU ARE HERE: {currentRoom.RoomName.ToUpper()}]");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  Connected Paths:");

            foreach (var choice in availableChoices)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("    └──> ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"[{choice.RoomName}]");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write(choice.IsMonitored ? " (SECURED CAMERA PATH) " : " (UNMONITORED DUCT) ");
                Console.WriteLine();
            }
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("===================================================================\n");
            Console.ResetColor();
        }
    }
}