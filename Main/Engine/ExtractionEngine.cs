using System;
using CyberHeistAce.Engine;
using CyberHeistAce.Models;
using CyberHeistButuan.Models; // Maps to your models namespace
using CyberHeistButuan.Engine;
using CyberHeistAce.UI;  // Maps to your detection system namespace

namespace CyberHeistButuan.Engine
{
    public class ExfiltrationEngine
    {
        private readonly Player _player;
        private readonly Detection_System _detectionSystem;
        private readonly NavigationManager _navManager;
        private readonly Random _random = new Random();

        // Track session stats for the final summary screen
        private int _damageTaken = 0;
        private int _critsRolled = 0;
        private int _alarmsTriggered = 0;

        public ExfiltrationEngine(Player player, Detection_System detectionSystem, NavigationManager navManager)
        {
            _player = player;
            _detectionSystem = detectionSystem;
            _navManager = navManager;
        }

        /// <summary>
        /// Handles the vertical climb from the Maintenance Room Vents to the Rooftop Access Corridor.
        /// </summary>
        public bool HandleClimbingCheck()
        {
            TerminalRenderer.PrintAmbient("\nYou look up through a shaft at the end of the Maintenance Room Vents.");
            TerminalRenderer.PrintAmbient("A vertical climbing shaft with high-pressure steam pipes leads to the Roof Access Corridor.");
            TerminalRenderer.PrintAmbient("Scaling this shaft silently requires physical coordination (DC 14 Stealth check).");
            Console.Write("\nPress [Enter] to roll...");
            Console.ReadLine();

            // Medium-Hard Sneak Check
            var roll = Dice_Roller.RollD20(_player.EffectiveSneakPTS, 14);
            Terminal_Render.PrintRoll($"Stealth Climbing Check (DC 14): D20 + {_player.EffectiveSneakPTS} (Base Roll: {roll.BaseRoll}) = {roll.Total}");

            if (roll.IsNat20) _critsRolled++;

            if (roll.IsSuccess)
            {
                Terminal_Render.PrintSuccess("PASS! You scale the shaft silently and pull yourself into the Rooftop Access Corridor.");
                _navManager.MoveTo("rooftop_access_corridor");
                return true;
            }
            else
            {
                Terminal_Render.PrintAlarm("FAIL! You slip on a slick structural brace, falling back into the vents with a loud clang!");
                _detectionSystem.RecordCheckResult(false);
                _alarmsTriggered++;
                return false;
            }
        }

        /// <summary>
        /// Executes the 3-Turn Rooftop Extraction sequence.
        /// </summary>
        public bool TriggerRooftopExtraction()
        {
            Terminal_Render.PrintAlarm("\n=======================================================");
            Terminal_Render.PrintAlarm("       WARNING: ROOFTOP HELIPAD EXTRACTION PROTOCOL     ");
            Terminal_Render.PrintAlarm("=======================================================");
            Terminal_Render.PrintAmbient("An automated roof defense grid is warming up and guard units are responding.");
            Terminal_Render.PrintAmbient("You must secure the extraction zone over 3 consecutive turns.\n");

            // --- TURN 1: Hack Flight Computer ---
            Terminal_Render.PrintAmbient("[TURN 1/3] OVERRIDE CHOPPER FLIGHT ENCRYPTION");
            Terminal_Render.PrintAmbient("The chopper transponder must be configured before landing (DC 14 Hack check).");
            Console.Write("Press [Enter] to roll...");
            Console.ReadLine();

            var turn1Roll = Dice_Roller.RollD20(_player.HackPTS, 14);
            if (turn1Roll.IsNat20) _critsRolled++;
            Terminal_Render.PrintRoll($"Hacking Flight Computer (DC 14): D20 + {_player.HackPTS} (Base Roll: {turn1Roll.BaseRoll}) = {turn1Roll.Total}");

            if (!turn1Roll.IsSuccess)
            {
                Terminal_Render.PrintAlarm("\nFAILURE! The chopper security firewall locks down, triggering a security broadcast.");
                _alarmsTriggered++;
                EscalateToCombat();
                return false;
            }
            Terminal_Render.PrintSuccess("PASS! Transponder cracked. Extraction flight route uploaded successfully.\n");

            // --- TURN 2: Hold Position ---
            Terminal_Render.PrintAmbient("[TURN 2/3] HOLD POSITION AGAINST OUTPOST TURRETS");
            Terminal_Render.PrintAmbient("Roof turrets lock onto the landing zone! Fire back or find quick cover (DC 15 Fight or Sneak check).");
            Console.WriteLine("Choose your approach:\n [1] Fight Back (fightPTS)\n [2] Dodge and Evale (Effective sneakPTS)");
            Console.Write("Choice: ");
            string strategy = Console.ReadLine() ?? "";

            RollResult turn2Roll;
            if (strategy == "1")
            {
                turn2Roll = Dice_Roller.RollD20(_player.FightPTS, 15);
                Terminal_Render.PrintRoll($"Suppressing Targets (DC 15): D20 + {_player.FightPTS} (Base Roll: {turn2Roll.BaseRoll}) = {turn2Roll.Total}");
            }
            else
            {
                turn2Roll = Dice_Roller.RollD20(_player.EffectiveSneakPTS, 15);
                Terminal_Render.PrintRoll($"Evasion Strategy (DC 15): D20 + {_player.EffectiveSneakPTS} (Base Roll: {turn2Roll.BaseRoll}) = {turn2Roll.Total}");
            }

            if (turn2Roll.IsNat20) _critsRolled++;

            if (!turn2Roll.IsSuccess)
            {
                Terminal_Render.PrintAlarm("\nFAILURE! You are exposed. Defense grids catch you in crossfire!");
                int damage = _random.Next(4, 9); // Heavy turret damage
                _player.CurrentHP -= damage;
                _damageTaken += damage;
                Terminal_Render.PrintAlarm($"You sustain {damage} damage! Current HP: {Math.Max(0, _player.CurrentHP)}/{_player.MaxHP}");

                if (_player.CurrentHP <= 0)
                {
                    Terminal_Render.PrintAlarm("\n[PERMADEATH] You succumbed to injuries while defending the landing pad. GAME OVER.");
                    Environment.Exit(0);
                }

                _alarmsTriggered++;
                EscalateToCombat();
                return false;
            }
            Terminal_Render.PrintSuccess("PASS! You successfully hold position as the extraction doors slide open.\n");

            // --- TURN 3: Signal Escape ---
            Terminal_Render.PrintAmbient("[TURN 3/3] FINAL LEAP AND SIGNAL ESCAPE");
            Terminal_Render.PrintAmbient("The rotor blades are turning. Leap aboard and clear the zone!");
            Console.Write("Press [Enter] to vault into the chopper...");
            Console.ReadLine();

            Terminal_Render.PrintSuccess("\n=======================================================");
            Terminal_Render.PrintSuccess("                     ESCAPE SUCCESSFUL!                ");
            Terminal_Render.PrintSuccess("=======================================================");
            
            PrintVictoryMetrics();
            return true;
        }

        private void EscalateToCombat()
        {
            _detectionSystem.SetState(DetectionState.In_Encounter);
            Terminal_Render.PrintAlarm("\n[COMBAT INITIATED] Defense protocols failed! Guards are storming the roof.");
        }

        private void PrintVictoryMetrics()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"
 __      _______ _____ _______ ____  _______     __
 \ \    / /_   _/ ____|__   __/ __ \|  __ \ \   / /
  \ \  / /  | || |       | | | |  | | |__) \ \_/ / 
   \ \/ /   | || |       | | | |  | |  _  / \   /  
    \  /   _| || |____   | | | |__| | | \ \  | |   
     \/   |_____\_____|  |_|  \____/|_|  \_\ |_|   ");
            Console.ResetColor();

            Terminal_Render.PrintSuccess("\n--- CYBER HEIST ACE EVALUATION ---");
            Terminal_Render.PrintAmbient($"Hacker Alias: {_player.Name}");
            Terminal_Render.PrintAmbient($"Alarms Registered: {_alarmsTriggered}");
            Terminal_Render.PrintAmbient($"Vitals Lost: {_damageTaken} HP");
            Terminal_Render.PrintAmbient($"Critical D20 Rolls: {_critsRolled}");
            Terminal_Render.PrintSuccess("----------------------------------");
            Terminal_Render.PrintAmbient("\nYou escaped with the database contents. Session concluded.");
            Environment.Exit(0);
        }
    }
}