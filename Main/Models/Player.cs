using System;
using System.Collections.Generic;

namespace CyberHeistButuan.Models
{
    public class Player
    {
        public string Name { get; set; } = "Netrunner";
        public int MaxHP { get; set; } = 20;

        private double _currentHP = 20;
        public double CurrentHP
        {
            get => _currentHP;
            set
            {
                if (value < _currentHP)
                {
                    TotalDamageSustained += (_currentHP - value);
                }
                _currentHP = value;
            }
        }
        
        // Base stats
        public int HackPTS { get; set; } = 1;

        private int _sneakPTS = 1;
        public int SneakPTS
        {
            get => IsMoist ? Math.Max(1, _sneakPTS - 2) : _sneakPTS;
            set => _sneakPTS = value;
        }

        public int FightPTS { get; set; } = 1;

        // Status Effects
        public bool IsMoist { get; set; } = false;

        // Exfiltration & Stats Tracking
        public int TotalTurns { get; set; } = 0;
        public int StealthPasses { get; set; } = 0;
        public int StealthFails { get; set; } = 0;
        public double TotalDamageSustained { get; private set; } = 0;

        // Inventory system (Capped at 2 snacks)
        public List<Item> Inventory { get; set; } = new List<Item>();
        public const int MaxInventorySize = 2;

        public void AllocatePoints()
        {
            int totalPoints = 10;
            Console.WriteLine("\n--- Character Initialization: Stat Point Allocation ---");
            Console.WriteLine($"Allocate {totalPoints} total points between hackPTS, sneakPTS, and fightPTS.");
            Console.WriteLine("Allocation Rules:");
            Console.WriteLine("1) Minimum of 1 point must be assigned to any single stat.");
            Console.WriteLine("2) Maximum of 6 points can be assigned to any single stat.");
            Console.WriteLine("3) Total allocated points across all stats must sum exactly to 10.");

            while (true)
            {
                int hack = PromptForStat("hackPTS");
                int sneak = PromptForStat("sneakPTS");
                int fight = PromptForStat("fightPTS");

                if (hack + sneak + fight == totalPoints)
                {
                    HackPTS = hack;
                    SneakPTS = sneak;
                    FightPTS = fight;
                    Console.WriteLine($"\nStats allocated. hackPTS: {HackPTS} | sneakPTS: {SneakPTS} | fightPTS: {FightPTS}\n");
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: Sum of entered points ({hack + sneak + fight}) does not equal {totalPoints}. Please try again.");
                    Console.ResetColor();
                }
            }
        }

        private int PromptForStat(string statName)
        {
            while (true)
            {
                Console.Write($"Enter points for {statName} (1-6): ");
                string? input = Console.ReadLine();
                
                if (int.TryParse(input, out int value))
                {
                    if (value < 1 || value > 6)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error: Points must fall within the range of 1 to 6.");
                        Console.ResetColor();
                        continue;
                    }
                    return value;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: Invalid entry. Please enter an integer.");
                    Console.ResetColor();
                }
            }
        }

        public bool AddItem(Item item)
        {
            if (Inventory.Count >= MaxInventorySize)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Inventory full. Cannot store snack.");
                Console.ResetColor();
                return false;
            }
            Inventory.Add(item);
            Console.WriteLine($"Stowed {item.Name} in your inventory.");
            return true;
        }
    }
}