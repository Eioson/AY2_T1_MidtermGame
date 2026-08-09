using System;
using   Main.Models;

namespace Main.Engine
{
    public class MapManager
    {
        public Room CurrentRoom { get; private set; }

        public void InitializeMap()
        {
            // Declare Rooms
            Room lobby = new Room("R1", "ACE Hospital Main Entrance", "A slick lobby lined with security drones.");
            Room vents = new Room("R2", "Ventilation Shaft", "Dark, dusty, and safe from patrolling guards.");
            Room server = new Room("R3", "Mainframe Server Room", "Target room humming with blue neon lights.");

            // Connect Rooms
            lobby.Exits.Add(1, ("Sneak into Maintenance Vent", vents, "sneakPTS", 10));
            lobby.Exits.Add(2, ("Slice Main Vault Lock", server, "hackPTS", 15));

            vents.Exits.Add(1, ("Drop down into Mainframe Server Room", server, "sneakPTS", 12));
            vents.Exits.Add(2, ("Crawl back to Lobby", lobby, "none", 0));

            server.Exits.Add(1, ("Retreat back to Vents", vents, "sneakPTS", 10));

            CurrentRoom = lobby;
        }

        public void ProcessChoice(int choice, Player player)
        {
            if (!CurrentRoom.Exits.ContainsKey(choice))
            {
                Console.WriteLine("Invalid selection! Press any key to retry.");
                Console.ReadKey();
                return;
            }

            var selectedExit = CurrentRoom.Exits[choice];

            if (selectedExit.TargetDC > 0)
            {
                var (roll, isNat20, isNat1) = DiceRoller.RollD20();
                int statMod = selectedExit.RequiredStat switch
                {
                    "sneakPTS" => player.SneakPTS,
                    "hackPTS" => player.HackPTS,
                    "fightPTS" => player.FightPTS,
                    _ => 0
                };

                int total = roll + statMod;
                Console.WriteLine($"\nRolling d20 for {selectedExit.RequiredStat}... Rolled {roll} + {statMod} = {total} (DC {selectedExit.TargetDC})");

                if (isNat20 || total >= selectedExit.TargetDC)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[SUCCESS] Path cleared!");
                    Console.ResetColor();
                    CurrentRoom = selectedExit.TargetRoom;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("[FAILURE] Security detected you!");
                    Console.ResetColor();
                    player.CurrentState = DetectionState.Suspicious;
                    CurrentRoom = selectedExit.TargetRoom;
                }
                Console.ReadKey();
            }
            else
            {
                CurrentRoom = selectedExit.TargetRoom;
            }
        }
    }
}