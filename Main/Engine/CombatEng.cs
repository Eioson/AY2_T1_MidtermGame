using System;
using CyberHeistButuan.Models;

namespace CyberHeistButuan.Engine
{
    public class CombatEngine
    {
        private readonly Player _player;
        private readonly Detection_System _detectionSystem;
        private readonly Random _random = new Random();

        public int GuardHP { get; private set; } = 5;
        public int GuardCount { get; private set; } = 1;
        public int TurnCount { get; private set; } = 0;

        public CombatEngine(Player player, Detection_System detectionSystem)
        {
            _player = player;
            _detectionSystem = detectionSystem;
        }

        public void StartCombat()
        {
            GuardHP = 5;
            GuardCount = 1;
            TurnCount = 0;
            _detectionSystem.SetState(DetectionState.In_Encounter);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n[ALARM] IN ENCOUNTER: Direct combat engaged!");
            Console.ResetColor();
        }

        public bool ProcessCombatTurn(string choice)
        {
            TurnCount++;
            bool playerSkippedAttack = false;

            switch (choice)
            {
                case "1":
                    ExecuteSuppressedGun();
                    break;
                case "2":
                    ExecuteUnsuppressedCarbine();
                    break;
                case "3":
                    if (ExecuteSmokeScreen())
                    {
                        return true; // Successfully escaped combat back to Detected status
                    }
                    break;
                case "4":
                    playerSkippedAttack = ExecuteUseSnack();
                    break;
                default:
                    Console.WriteLine("Invalid entry. Your combat action is skipped.");
                    break;
            }

            // Verify if threat remains
            if (GuardHP <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nGuards eliminated. Threat cleared.");
                Console.ResetColor();
                _detectionSystem.SetState(DetectionState.Suspicious);
                return true; // Combat ends
            }

            // Enemy Phase
            ExecuteGuardsAttack(playerSkippedAttack);

            // Guard spawn evaluation (Every 4 turns, cap of 4 guards)
            if (TurnCount % 4 == 0)
            {
                SpawnReinforcement();
            }

            return false;
        }

        private void ExecuteSuppressedGun()
        {
            Console.WriteLine("\nFiring Suppressed Pistol...");
            var result = Dice_Roller.RollD20(_player.FightPTS, 15); // Medium DC (15)
            Console.WriteLine($"Rolled {result.BaseRoll} + {_player.FightPTS} = {result.Total} (DC 15)");

            if (result.IsSuccess)
            {
                int shot1 = _random.Next(1, 3);
                int shot2 = _random.Next(1, 3);
                int totalDmg = shot1 + shot2;
                GuardHP -= totalDmg;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"PASS! You land double quiet shots, dealing {shot1} and {shot2} damage (Total: {totalDmg} DMG)!");
                Console.ResetColor();
            }
            else
            {
                int dmg = _random.Next(0, 2); // 0-1 dmg on fail
                GuardHP -= dmg;
                if (dmg == 0)
                {
                    Console.WriteLine("FAIL! The attack missed!");
                }
                else
                {
                    Console.WriteLine($"FAIL! The shot grazed the guard, dealing {dmg} damage.");
                }
            }
            Console.WriteLine($"Remaining Guard Pool HP: {Math.Max(0, GuardHP)}");
        }

        private void ExecuteUnsuppressedCarbine()
        {
            Console.WriteLine("\nFiring Unsuppressed Carbine...");
            var result = Dice_Roller.RollD20(_player.FightPTS, 18); // Hard DC (18)
            Console.WriteLine($"Rolled {result.BaseRoll} + {_player.FightPTS} = {result.Total} (DC 18)");

            if (result.IsSuccess)
            {
                int dmg = _random.Next(4, 7); // 4-6 dmg
                GuardHP -= dmg;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"PASS! You fire indiscriminately, dealing {dmg} damage to the guards!");
                Console.ResetColor();
            }
            else
            {
                int dmg = _random.Next(1, 4); // 1-3 dmg
                GuardHP -= dmg;
                Console.WriteLine($"FAIL! You fire wildly, dealing {dmg} damage.");

                // 10% chance to self harm for 1-2 damage
                if (_random.Next(1, 101) <= 10)
                {
                    int selfDmg = _random.Next(1, 3);
                    _player.CurrentHP -= selfDmg;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Ouch! You take {selfDmg} points of self-damage from recoil/blowback.");
                    Console.ResetColor();
                }
            }
            Console.WriteLine($"Remaining Guard Pool HP: {Math.Max(0, GuardHP)}");
        }

        private bool ExecuteSmokeScreen()
        {
            Console.WriteLine("\nDeploying Smokescreen...");
            var result = Dice_Roller.RollD20(_player.SneakPTS, 10); // Easy DC (10)
            Console.WriteLine($"Rolled {result.BaseRoll} + {_player.SneakPTS} = {result.Total} (DC 10)");

            if (result.IsSuccess)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("PASS! You successfully deploy a smokescreen and break contact!");
                Console.ResetColor();
                _detectionSystem.SetState(DetectionState.Detected); // Returns to Detected state
                return true;
            }
            else
            {
                Console.WriteLine("FAIL! The smokescreen fails to deploy. You wasted your action!");
                return false;
            }
        }

        private bool ExecuteUseSnack()
        {
            if (_player.Inventory.Count == 0)
            {
                Console.WriteLine("You do not have any snacks in your inventory.");
                return false;
            }

            var snack = _player.Inventory[0];
            _player.Inventory.RemoveAt(0);

            if (snack.Type == ItemType.LightSnack)
            {
                int heal = _random.Next(1, 4);
                _player.CurrentHP = Math.Min(_player.MaxHP, _player.CurrentHP + heal);
                Console.WriteLine($"Consumed Light Snack. Healed {heal} HP. Current HP: {_player.CurrentHP}/{_player.MaxHP}.");
                return true;
            }
            else if (snack.Type == ItemType.HeavySnack)
            {
                _player.CurrentHP = _player.MaxHP;
                Console.WriteLine($"Consumed Heavy Snack. Healed to FULL HP ({_player.MaxHP}).");
                Console.WriteLine("Warning: Guards catch you in a delay, preparing a free counterattack strike.");
                return true; // Triggers skipped combat turn and free guard hit
            }

            return false;
        }

        private void ExecuteGuardsAttack(bool playerSkippedAttack)
        {
            Console.WriteLine("\n--- Guard Counterattack ---");
            int dmg = _random.Next(0, 3);
            _player.CurrentHP -= dmg;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Guards fire on you, dealing {dmg} damage.");
            Console.ResetColor();

            if (playerSkippedAttack)
            {
                int extraDmg = _random.Next(0, 3);
                _player.CurrentHP -= extraDmg;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Heavy eating penalty! Guard free hit inflicts an additional {extraDmg} damage.");
                Console.ResetColor();
            }

            Console.WriteLine($"Your HP: {Math.Max(0, _player.CurrentHP)}/{_player.MaxHP}");
        }

        private void SpawnReinforcement()
        {
            if (GuardCount < 4)
            {
                GuardCount++;
                GuardHP += 5; // Clumps and stacks
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[REINFORCEMENT] A guard joins the fray! Guards count: {GuardCount}. Pool HP updated to: {GuardHP}");
                Console.ResetColor();
            }
        }
    }
}