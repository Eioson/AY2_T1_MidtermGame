using System;
using System.Collections.Generic;
<<<<<<< HEAD
using CyberHeistButuan.Models;

namespace CyberHeistAce.Models
{
    public class Player
    {
        public string Name { get; set; } = "Hacker";
=======

namespace CyberHeistButuan.Models
{
    public class Player
    {
        public string Name { get; set; } = "Netrunner";
>>>>>>> 993e84fb50430dedc72f23580b65cf9526fa6fed
        public int MaxHP { get; set; } = 20;
        public double CurrentHP { get; set; } = 20;
        
        // Base stats
        public int HackPTS { get; set; } = 1;
        public int SneakPTS { get; set; } = 1;
        public int FightPTS { get; set; } = 1;

<<<<<<< HEAD
        // Status Effects
        public bool IsMoist { get; set; } = false; // Inflicts squeaky shoes stealth penalty
        public bool IsHot { get; set; } = false;   // Inflicts gradual HP loss

        // Turn counters for environmental cycles
        private int _hotTurnsCount = 0;

        /// <summary>
        /// Returns the sneaker's stealth modifier after calculating active status penalties.
        /// </summary>
        public int EffectiveSneakPTS => IsMoist ? Math.Max(1, SneakPTS - 1) : SneakPTS;

        // Inventory system (Capped at 2 snacks)
        public List<Item> Inventory { get; set; } = new List<Item>();
        public const int MaxInventorySize = 2;

=======
        // Inventory system (Capped at 2 snacks)
        public List<Item> Inventory { get; set; } = new List<Item>();
        public const int MaxInventorySize = 2;

>>>>>>> 993e84fb50430dedc72f23580b65cf9526fa6fed
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
<<<<<<< HEAD
        }

        /// <summary>
        /// Evaluates active environmental damage cycles.
        /// </summary>
        public void ApplyTurnHazards()
        {
            if (IsHot)
            {
                _hotTurnsCount++;
                // Drains 1 HP every 2 turns (representing the 0.5 HP loss per turn rate)
                if (_hotTurnsCount % 2 == 0)
                {
                    CurrentHP = Math.Max(0, CurrentHP - 1);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n[HAZARD ALARM] Blistering heat from steam vents drains your vitals! You lost 1 HP. (Current HP: {CurrentHP}/{MaxHP})");
                    Console.ResetColor();

                    if (CurrentHP <= 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\n[PERMADEATH] You collapsed from heat exhaustion. GAME OVER.");
                        Console.ResetColor();
                        Environment.Exit(0);
                    }
                }
            }
            else
            {
                _hotTurnsCount = 0; // Reset counter when exiting hot zones
            }
=======
>>>>>>> 993e84fb50430dedc72f23580b65cf9526fa6fed
        }
    }
}