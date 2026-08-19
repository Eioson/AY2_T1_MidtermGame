using System;
using CyberHeistButuan.Models;
using CyberHeistButuan.UI;

namespace CyberHeistButuan.Engine
{
    public class ExfiltrationEngine
    {
        private readonly Player _player;
        private readonly Detection_System _detectionSystem;
        private readonly CombatEngine _combat;

        public bool IsBeaconSignaled { get; private set; } = false;
        public int ChopperETA { get; private set; } = 3;
        public bool IsChopperArrived => IsBeaconSignaled && ChopperETA <= 0;

        public ExfiltrationEngine(Player player, Detection_System detectionSystem, CombatEngine combat)
        {
            _player = player;
            _detectionSystem = detectionSystem;
            _combat = combat;
        }

        public void ProcessHelipadTurn()
        {
            if (IsBeaconSignaled && ChopperETA > 0)
            {
                ChopperETA--;
                if (ChopperETA == 0)
                {
                    Terminal_Render.PrintSuccess("\n[HELICOPTER] The exfiltration chopper has landed on the helipad! BOARD NOW!");
                }
                else
                {
                    Terminal_Render.PrintRoll($"\n[HELICOPTER] Helicopter en route. ETA: {ChopperETA} turn(s) remaining.");
                    
                    if (_detectionSystem.CurrentState == DetectionState.In_Encounter || _detectionSystem.CurrentState == DetectionState.Detected)
                    {
                        Terminal_Render.PrintAlarm("[WARNING] High alert! Guard reinforcement sweep incoming on the roof!");
                        if (_detectionSystem.CurrentState != DetectionState.In_Encounter)
                        {
                            _detectionSystem.SetState(DetectionState.In_Encounter);
                        }
                    }
                }
            }
        }

        public bool AttemptSignal()
        {
            if (IsBeaconSignaled)
            {
                Console.WriteLine("The helicopter has already been signaled.");
                return false;
            }

            Console.Clear();
            ASCII_Art.DisplayTitleBanner();
            Console.WriteLine("================================================================================");
            Console.WriteLine("                EXFILTRATION BEACON SIGNAL SYSTEM                               ");
            Console.WriteLine("================================================================================");
            Console.WriteLine("You stand before the helipad beacon terminal. To summon the exfiltration chopper,");
            Console.WriteLine("you must override the hospital's localized tracking frequency.");
            Console.WriteLine("\nChoose exfiltration signaling method:");
            Console.WriteLine(" [1] Overload Beacon Node (DC 15 - hackPTS) - Quiet exfiltration call.");
            Console.WriteLine(" [2] Deploy Flare Gun (DC 13 - sneakPTS)   - Fast but highly visible.");
            Console.Write("\nSelect method: ");
            
            string choice = Console.ReadLine() ?? "";
            
            if (choice == "1")
            {
                var roll = Dice_Roller.RollD20(_player.HackPTS, 15);
                Terminal_Render.PrintRoll($"\nHacking Beacon Terminal (DC 15): D20 + {_player.HackPTS} (Roll: {roll.BaseRoll}) = {roll.Total}");
                if (roll.IsSuccess)
                {
                    Terminal_Render.PrintSuccess("SUCCESS! Beacon silently slaved. Coordinates broadcast. Chopper ETA: 3 turns.");
                    IsBeaconSignaled = true;
                    ChopperETA = 3;
                }
                else
                {
                    Terminal_Render.PrintAlarm("FAILURE! Beacon firewall triggered a localized silent alarm. Reinforcements warned!");
                    _detectionSystem.RecordCheckResult(false);
                    _detectionSystem.SetState(DetectionState.Detected);
                    IsBeaconSignaled = true;
                    ChopperETA = 4;
                }
                Console.ReadKey(true);
                return true;
            }
            else if (choice == "2")
            {
                var roll = Dice_Roller.RollD20(_player.SneakPTS, 13);
                Terminal_Render.PrintRoll($"\nFiring Flare Gun (DC 13): D20 + {_player.SneakPTS} (Roll: {roll.BaseRoll}) = {roll.Total}");
                if (roll.IsSuccess)
                {
                    Terminal_Render.PrintSuccess("SUCCESS! You fire the flare into the sky. Chopper is incoming! ETA: 2 turns.");
                    _detectionSystem.SetState(DetectionState.Detected);
                    IsBeaconSignaled = true;
                    ChopperETA = 2;
                }
                else
                {
                    Terminal_Render.PrintAlarm("FAILURE! The flare misfires, immediately pinpointing your location!");
                    _detectionSystem.SetState(DetectionState.In_Encounter);
                    IsBeaconSignaled = true;
                    ChopperETA = 3;
                }
                Console.ReadKey(true);
                return true;
            }
            else
            {
                Console.WriteLine("Invalid choice. Aborting signal attempt.");
                Console.ReadKey(true);
                return false;
            }
        }

        public void TriggerVictoryExfiltration()
        {
            Console.Clear();
            ASCII_Art.DisplayTitleBanner();
            Terminal_Render.PrintSuccess("\n================================================================================");
            Terminal_Render.PrintSuccess("                HEIST COMPLETE - EXFILTRATION SUCCESSFUL!                      ");
            Terminal_Render.PrintSuccess("================================================================================");
            Terminal_Render.PrintAmbient("The exfiltration helicopter banks sharply over the glittering skyline of the City.\n" +
                                         "Behind you, the sirens of ACE Hospital fade into the rain. You have escaped with the data.", true);

            Console.WriteLine("\n------------------------- MISSION PERFORMANCE EVALUATION -------------------------");
            Console.WriteLine($"Total Turns Elapsed:       {_player.TotalTurns} turn(s)");
            Console.WriteLine($"Total Damage Sustained:    {_player.TotalDamageSustained:F1} HP");
            Console.WriteLine($"Successful Stealth Checks: {_player.StealthPasses}");
            Console.WriteLine($"Failed Stealth Checks:     {_player.StealthFails}");

            double stealthRatio = (_player.StealthPasses + _player.StealthFails) == 0 ? 1.0 : (double)_player.StealthPasses / (_player.StealthPasses + _player.StealthFails);
            string finalGrade;

            if (_player.TotalDamageSustained == 0 && stealthRatio >= 0.8)
            {
                finalGrade = "S (Ghost / Perfect Run)";
            }
            else if (_player.TotalDamageSustained < 8 && stealthRatio >= 0.6)
            {
                finalGrade = "A (Professional)";
            }
            else if (_player.TotalDamageSustained < 15)
            {
                finalGrade = "B (Smash & Grab)";
            }
            else
            {
                finalGrade = "C (Survivalist)";
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nFINAL HEIST GRADE:         {finalGrade}");
            Console.ResetColor();
            Console.WriteLine("================================================================================");
            Console.WriteLine("Press any key to exit Cyber Heist Ace...");
            Console.ReadKey(true);
        }
    }
}