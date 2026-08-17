using System;

namespace CyberHeistButuan.UI
{
    public static class ASCII_Art
    {
        public static void DisplayTitleBanner()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(@"
================================================================================
   ______     __               __  __     _     __     ____        __                  
  / ____/_  _/ /_  ___  _____ / / / /__  (_)___/ /_   / __ )__  __/ /_  ______ _____   
 / /   / / / / __ \/ _ \/ ___// /_/ / _ \/ / ___/ __/  / __  / / / / __/ / / / __ `/ __ \ 
/ /___/ /_/ / /_/ /  __/ /   / __  /  __/ (__  ) /_   / /_/ / /_/ / /_/ /_/ / /_/ / / / /
\____/\__, /_.___/\___/_/   /_/ /_/\___/_/____/\__/  /_____/\__,_/\__/\__,_/\__,_/_/ /_/ 
     /____/                                                                              
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
    }
}