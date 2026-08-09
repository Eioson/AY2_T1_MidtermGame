using System;
using Main.Engine;
using Main.Models;

class Program
{
    static void Main(string[] args)
    {
        Console.Title = "Cyber-Heist 2088";

        Player player = new Player("Netrunner");
        MapManager map = new MapManager();
        map.InitializeMap();

        bool running = true;

        while (running && player.CurrentHP > 0)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"=== LOCATION: {map.CurrentRoom.Name.ToUpper()} ===");
            Console.ResetColor();
            Console.WriteLine(map.CurrentRoom.Description);
            Console.WriteLine($"[HP: {player.CurrentHP}/{player.MaxHP} | State: {player.CurrentState}]\n");

            Console.WriteLine("Options:");
            foreach (var exit in map.CurrentRoom.Exits)
            {
                string checkInfo = exit.Value.TargetDC > 0 ? $" [{exit.Value.RequiredStat} Check - DC {exit.Value.TargetDC}]" : "";
                Console.WriteLine($"  [{exit.Key}] {exit.Value.ChoiceText}{checkInfo}");
            }

            Console.Write("\nEnter choice: ");
            if (int.TryParse(Console.ReadLine(), out int input))
            {
                map.ProcessChoice(input, player);
            }
        }
    }
}