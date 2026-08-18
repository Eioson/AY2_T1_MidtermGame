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
   ______      __                 __  __     _      __     ___                
  / ____/_  __/ /_  ___  _____   / / / /__  (_)____/ /_   /   | ________      
 / /   / / / / __ \/ _ \/ ___/  / /_/ / _ \/ / ___/ __/  / /| |/ ___/ _ \     
/ /___/ /_/ / /_/ /  __/ /     / __  /  __/ (__  ) /_   / ___ / /__/  __/     
\____/\__, /_.___/\___/_/     /_/ /_/\___/_/____/\__/  /_/  |_\___/\___/      
     /____/                                                                           
================================================================================");
            Console.ResetColor();
        }

        public static void DisplayHospitalMap()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(@"");
            Console.ResetColor();
        }
    }
}