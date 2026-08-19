﻿using System;
using System.IO;
using CyberHeistButuan.Models;
using CyberHeistButuan.Engine;
using CyberHeistButuan.UI;

namespace CyberHeistButuan
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            ASCII_Art.DisplayTitleBanner();

            Terminal_Render.PrintAmbient("System initialization sequence online. Connecting to local server networks...", true);

            // Establish Player profile
            Player player = new Player();
            player.AllocatePoints();

            // Set up initial starter pack
            player.AddItem(new Item("Soy-Snack Light Pack", ItemType.LightSnack));
            player.AddItem(new Item("Chiba heavy energy paste", ItemType.HeavySnack));

            Detection_System detectionSystem = new Detection_System();
            Nav_Manager nav = new Nav_Manager();

            // Attempt file layout loading, with string fallback setup if missing
            string jsonContent = "";
            if (File.Exists("Map.json"))
            {
                jsonContent = File.ReadAllText("Map.json");
            }
            else
            {
                jsonContent = GetMapBackupData();
            }

            nav.LoadMap(jsonContent);
            CombatEngine combat = new CombatEngine(player, detectionSystem);
            ExfiltrationEngine exfil = new ExfiltrationEngine(player, detectionSystem, combat);

            Terminal_Render.PrintAmbient("\nInitialization completed. Press any key to deploy to Outside area...", false);
            Console.ReadKey(true);

            bool heistRunning = true;
            bool dataExtracted = false;

            while (heistRunning)
            {
                Console.Clear();
                ASCII_Art.DisplayHospitalMap();
                RenderStatusHUD(player, detectionSystem, nav, dataExtracted);

                // Run tactical engagement checks if transition triggers combat
                if (detectionSystem.CurrentState == DetectionState.In_Encounter)
                {
                    combat.StartCombat();
                    while (detectionSystem.CurrentState == DetectionState.In_Encounter)
                    {
                        Console.Clear();
                        Terminal_Render.PrintAlarm("=================== DIRECT COMBAT REGISTERED ===================");
                        Console.WriteLine($"Guards deployed: {combat.GuardCount} | Total Stack HP: {combat.GuardHP}");
                        Console.WriteLine($"Your HP status: {player.CurrentHP}/{player.MaxHP}");
                        Console.WriteLine("\nCombat action selections:");
                        Console.WriteLine(" [1] Fire Suppressed Pistol (DC 15 - fightPTS)");
                        Console.WriteLine(" [2] Fire Unsuppressed Carbine (DC 18 - fightPTS)");
                        Console.WriteLine(" [3] Deploy Smokescreen escape (DC 10 - sneakPTS)");
                        Console.WriteLine($" [4] Consume Item (Snacks left: {player.Inventory.Count})");

                        Console.Write("\nInput action choice: ");
                        string option = Console.ReadLine() ?? "";
                        player.TotalTurns++;
                        bool escaped = combat.ProcessCombatTurn(option);

                        if (escaped)
                        {
                            Terminal_Render.PrintSuccess("\nEscaped direct engagement. Repositioning...");
                            Console.ReadKey(true);
                            break;
                        }
                        Console.WriteLine("\nPress any key to execute next action...");
                        Console.ReadKey(true);
                    }
                    continue;
                }

                // Standard Exploration phase
                var options = nav.GetConnections();
                Console.WriteLine($"\n[LOCATION] {nav.CurrentRoom.RoomName}");
                Terminal_Render.PrintAmbient(nav.CurrentRoom.Description);

                // Mainframe terminal interface
                if (nav.CurrentRoom.RoomId == "mainframe_room" && !dataExtracted)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[DECRYPT ACTION] [H] Attempt digital database extraction (DC 15 - hackPTS)");
                    Console.ResetColor();
                }

                // Helipad exfiltration choices
                if (nav.CurrentRoom.RoomId == "rooftop_helipad")
                {
                    if (!exfil.IsBeaconSignaled)
                    {
                        if (dataExtracted)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\n[EXFIL ACTION] [S] Hack Helipad Beacon and call extraction chopper");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine("\n[EXFIL LOCKED] [S] Hack Helipad Beacon (Retrieve Data Core first)");
                            Console.ResetColor();
                        }
                    }
                    else if (exfil.IsChopperArrived)
                    {
                        if (dataExtracted)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("\n[EXFIL ACTION] [B] Board helicopter and escape Butuan City!");
                            Console.ResetColor();
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGray;
                            Console.WriteLine("\n[EXFIL LOCKED] [B] Board helicopter (Retrieve Data Core first)");
                            Console.ResetColor();
                        }
                    }
                }

                bool insideVents = nav.CurrentRoom.RoomId.Contains("vent", StringComparison.OrdinalIgnoreCase);

                if (insideVents)
                {
                    var (ventShafts, dropDownRooms) = nav.GetVentNavigationOptions();

                    Console.WriteLine("\n--- VENT SHAFT NAVIGATION SYSTEM ---\n");
                    
                    Console.WriteLine("Crawl to connected vents:\n");
                    for (int i = 0; i < ventShafts.Count; i++)
                    {
                        var destination = ventShafts[i];
                        string details = destination.IsMonitored ? $"[MONITORED - DC {destination.BaseDC}]" : "[UNMONITORED]";
                        Console.Write($" [{i + 1}] Crawl to {destination.RoomName} ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(details);
                        Console.ResetColor();
                        Console.WriteLine();
                    }

                    Console.WriteLine("\nDrop down:\n");
                    for (int i = 0; i < dropDownRooms.Count; i++)
                    {
                        int choiceNumber = ventShafts.Count + i + 1;
                        var destination = dropDownRooms[i];
                        string details = destination.IsMonitored ? $"[MONITORED - DC {destination.BaseDC}]" : "[UNMONITORED]";
                        if (destination.RoomId == "rooftop_helipad" && !dataExtracted)
                        {
                            details = "[LOCKED - RETRIEVE DATA CORE FIRST]";
                        }
                        if (destination.RoomId == "rooftop_helipad" && exfil.IsBeaconSignaled && !exfil.IsChopperArrived)
                        {
                            details = $"[LOCKED - CHOPPER ETA: {exfil.ChopperETA} turn(s)]";
                        }
                        Console.Write($" [{choiceNumber}] Drop down into {destination.RoomName} ");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(details);
                        Console.ResetColor();
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine("\nPaths available:");
                    for (int i = 0; i < options.Count; i++)
                    {
                        var destination = options[i];
                        Console.Write($" [{i + 1}] Move to {destination.RoomName} ");

                        // Temporarily set foreground to yellow for the monitored status text
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        if (destination.IsMonitored)
                        {
                            Console.Write($"[MONITORED - DC {destination.BaseDC}]");
                        }
                        else
                        {
                            Console.Write("[UNMONITORED]");
                        }
                        Console.ResetColor();
                        Console.WriteLine();
                    }
                }
                Console.WriteLine(" [Q] Abort mission (Quit)");

                // Show option to wait if inside vents
                bool isInVents = nav.CurrentRoom.RoomId.EndsWith("vents", StringComparison.OrdinalIgnoreCase);
                if (isInVents)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(" [W] Wait quietly in the vents (Pass turn)");
                    Console.ResetColor();
                }

                Console.Write("\nChoose path: ");
                string entryChoice = Console.ReadLine() ?? "";

                if (entryChoice.Equals("Q", StringComparison.OrdinalIgnoreCase))
                {
                    heistRunning = false;
                    Terminal_Render.PrintAmbient("Exiting system connection. Heist aborted.", true);
                    break;
                }

                // Handle exfiltration triggers
                if (entryChoice.Equals("S", StringComparison.OrdinalIgnoreCase) && nav.CurrentRoom.RoomId == "rooftop_helipad" && !exfil.IsBeaconSignaled && dataExtracted)
                {
                    player.TotalTurns++;
                    exfil.AttemptSignal();
                    continue;
                }

                if (entryChoice.Equals("B", StringComparison.OrdinalIgnoreCase) && nav.CurrentRoom.RoomId == "rooftop_helipad" && exfil.IsChopperArrived && dataExtracted)
                {
                    exfil.TriggerVictoryExfiltration();
                    heistRunning = false;
                    break;
                }

                // Handle waiting in the vents
                if (entryChoice.Equals("W", StringComparison.OrdinalIgnoreCase) && isInVents)
                {   
                    Terminal_Render.PrintAmbient("\nYou wait quietly in the dark vents, letting time pass...");
                    player.TotalTurns++;
                    detectionSystem.ProcessTurn();
                    exfil.ProcessHelipadTurn();
                    Console.ReadKey(true);
                    continue;
                }

                if (entryChoice.Equals("H", StringComparison.OrdinalIgnoreCase) && nav.CurrentRoom.RoomId == "mainframe_room" && !dataExtracted)
                {
                    player.TotalTurns++;
                    var roll = Dice_Roller.RollD20(player.HackPTS, 15);
                    Terminal_Render.PrintRoll($"\nHacking Server Decryption (DC 15): D20 + {player.HackPTS} (Roll: {roll.BaseRoll}) = {roll.Total}");
                    if (roll.IsSuccess)
                    {
                        Terminal_Render.PrintSuccess("PASS! Decryption protocols cracked. Data core copied successfully.");
                        dataExtracted = true;
                    }
                    else
                    {
                        Terminal_Render.PrintAlarm("FAIL! Server firewall issues detected. Alarm alert levels rising!");
                        detectionSystem.RecordCheckResult(false);
                    }
                    Console.ReadKey(true);
                    continue;
                }

                if (int.TryParse(entryChoice, out int targetIndex) && targetIndex > 0 && targetIndex <= options.Count)
                {
                    RoomNode targetDestination;
                    if (insideVents)
                    {
                        var (ventShafts, dropDownRooms) = nav.GetVentNavigationOptions();
                        if (targetIndex <= ventShafts.Count)
                        {
                            targetDestination = ventShafts[targetIndex - 1];
                        }
                        else
                        {
                            targetDestination = dropDownRooms[targetIndex - ventShafts.Count - 1];
                        }
                    }
                    else
                    {
                        targetDestination = options[targetIndex - 1];
                    }

                    player.TotalTurns++;

                    if (nav.RequiresSneakCheck(targetDestination))
                    {
                        Terminal_Render.PrintAmbient($"\nAttempting silent entry into {targetDestination.RoomName}...", false);
                        var check = Dice_Roller.RollD20(player.SneakPTS, targetDestination.BaseDC);
                        Terminal_Render.PrintRoll($"Stealth Check (DC {targetDestination.BaseDC}): D20 + {player.SneakPTS} (Roll: {check.BaseRoll}) = {check.Total}");

                        if (check.IsSuccess)
                        {
                            Terminal_Render.PrintSuccess("PASS! Slipped through security monitors cleanly.");
                            player.StealthPasses++;
                            detectionSystem.RecordCheckResult(true);
                            nav.MoveTo(targetDestination.RoomId, player);
                        }
                        else
                        {
                            Terminal_Render.PrintAlarm("FAIL! Motion alarms triggered by the checkpoint monitors.");
                            player.StealthFails++;
                            detectionSystem.RecordCheckResult(false);
                            nav.MoveTo(targetDestination.RoomId, player);
                        }
                    }
                    else
                    {
                        nav.MoveTo(targetDestination.RoomId, player);
                        Terminal_Render.PrintAmbient($"\nMoved quietly into {targetDestination.RoomName}.");
                    }

                    detectionSystem.ProcessTurn();
                    exfil.ProcessHelipadTurn();
                    Console.ReadKey(true);
                }
                else
                {
                    Console.WriteLine("Invalid entry. Press any key to continue...");
                    Console.ReadKey(true);
                }

                // Check escape parameters
                if (nav.CurrentRoom.RoomId == "outside" && dataExtracted)
                {
                    Console.Clear();
                    ASCII_Art.DisplayTitleBanner();
                    Terminal_Render.PrintSuccess("\n========================================================");
                    Terminal_Render.PrintSuccess("   MISSION COMPLETE! EXTRACTION ARCHIVED SUCCESSFULLY!");
                    Terminal_Render.PrintSuccess("========================================================");
                    Terminal_Render.PrintAmbient("You successfully escape into the dark alleys of Butuan City with the hospital's private medical secrets.", true);
                    heistRunning = false;
                }
            }
        }

        private static void RenderStatusHUD(Player p, Detection_System ds, Nav_Manager nav, bool gotData)
        {
            Console.WriteLine("================================================================================");
            Console.Write($"HP: {p.CurrentHP:F1}/{p.MaxHP} | hackPTS: +{p.HackPTS} | sneakPTS: +{p.SneakPTS} | fightPTS: +{p.FightPTS}");
            
            if (p.IsMoist)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(" [STATUS: MOIST]");
                Console.ResetColor();
            }
            Console.Write(" | ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"Data Drive: ");
            switch (gotData)
            {
                case true:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("SECURED\n");
                    Console.ResetColor();
                    break;
                case false:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("MISSING\n");
                    Console.ResetColor();
                    break;
            }
    

            Console.Write("Alert Level: ");
            switch (ds.CurrentState)
            {
                case DetectionState.Undetected:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("UNDETECTED\n");
                    break;
                case DetectionState.Suspicious:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"SUSPICIOUS ({ds.GetSuspiciousTurnsRemaining()} turn(s) until cool down)\n");
                    break;
                case DetectionState.Detected:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"DETECTED ({ds.GetDetectedTurnsRemaining()} turn(s) until combat starts!)\n");
                    break;
                case DetectionState.In_Encounter:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("IN ENCOUNTER (Active Combat Alert)\n");
                    break;
            }
            Console.ResetColor();
            Console.WriteLine("================================================================================");
        }

        private static string GetMapBackupData()
        {
            return @"[
              {
                ""roomId"": ""outside"",
                ""roomName"": ""Outside ACE Hospital"",
                ""description"": ""Rain pours on the dark streets outside ACE Hospital in Butuan City. The building entrance lies ahead, and a vent layout is visible nearby."",
                ""baseDC"": 10,
                ""isMonitored"": false,
                ""connections"": [""lobby"", ""lobby_vents"", ""emergency_room""]
              },
              {
                ""roomId"": ""lobby"",
                ""roomName"": ""Main Lobby"",
                ""description"": ""A polished lobby with modern corporate aesthetics. A reception desk and security terminals stand near the entrance."",
                ""baseDC"": 10,
                ""isMonitored"": true,
                ""connections"": [
                  ""outside"",
                  ""employee_hallway_l1"",
                  ""lobby_vents"",
                  ""emergency_room""
                ]
              },
              {
                ""roomId"": ""emergency_room"",
                ""roomName"": ""Emergency Room"",
                ""description"": ""Littered with high-tech hospital apparatus and diagnostic beds. Quiet, but staff patrol regularly."",
                ""baseDC"": 15,
                ""isMonitored"": true,
                ""connections"": [""lobby"", ""lobby_vents""]
              },
              {
                ""roomId"": ""lobby_vents"",
                ""roomName"": ""Lobby Vents"",
                ""description"": ""A dark, cramped duct system positioned directly above the main reception lobby."",
                ""baseDC"": 10,
                ""isMonitored"": false,
                ""connections"": [
                  ""lobby"",
                  ""power_room_vents"",
                  ""mainframe_room_vents"",
                  ""maintenance_room_vents""
                ]
              },
              {
                ""roomId"": ""employee_hallway_l1"",
                ""roomName"": ""Employee Hallway (Level 1)"",
                ""description"": ""A brightly lit security hallway requiring credentials. Cameras sweep back and forth."",
                ""baseDC"": 10,
                ""isMonitored"": true,
                ""connections"": [""lobby"", ""employee_hallway_l2"", ""lobby_vents""]
              },
              {
                ""roomId"": ""employee_hallway_l2"",
                ""roomName"": ""Employee Hallway (Level 2)"",
                ""description"": ""A higher security restricted section. Power distribution and backup grid rooms connect to this floor."",
                ""baseDC"": 15,
                ""isMonitored"": true,
                ""connections"": [
                  ""employee_hallway_l1"",
                  ""employee_hallway_l3"",
                  ""power_room"",
                  ""lobby_vents""
                ]
              },
              {
                ""roomId"": ""employee_hallway_l3"",
                ""roomName"": ""Employee Hallway (Level 3)"",
                ""description"": ""Maximum security level. Red ambient lights reflect off the reinforced doors leading to the mainframe compartment."",
                ""baseDC"": 15,
                ""isMonitored"": true,
                ""connections"": [""employee_hallway_l2"", ""mainframe_doors"", ""lobby_vents"", ""rooftop_access_corridor""]
              },
              {
                ""roomId"": ""power_room"",
                ""roomName"": ""Power Room"",
                ""description"": ""Humming with electrical energy. Accessing systems here directly could control building subroutines."",
                ""baseDC"": 10,
                ""isMonitored"": false,
                ""connections"": [""employee_hallway_l2"", ""power_room_vents""]
              },
              {
                ""roomId"": ""mainframe_doors"",
                ""roomName"": ""Mainframe Door"",
                ""description"": ""A heavy blast door securing the hospital's central mainframe chamber."",
                ""baseDC"": 18,
                ""isMonitored"": true,
                ""connections"": [""employee_hallway_l3"", ""mainframe_room""]
              },
              {
                ""roomId"": ""mainframe_room"",
                ""roomName"": ""Mainframe Room"",
                ""description"": ""Rows of data racks contain private clinical and corporate secrets. The extraction computer is located here."",
                ""baseDC"": 15,
                ""isMonitored"": true,
                ""connections"": [""mainframe_doors"", ""mainframe_room_vents""]
              },
              {
                ""roomId"": ""power_room_vents"",
                ""roomName"": ""Power Room Vents"",
                ""description"": ""A metallic airway situated over the active generators of the power complex."",
                ""baseDC"": 10,
                ""isMonitored"": false,
                ""connections"": [""lobby_vents"", ""mainframe_room_vents"", ""power_room""]
              },
              {
                ""roomId"": ""mainframe_room_vents"",
                ""roomName"": ""Mainframe Room Vents"",
                ""description"": ""Vents tracking directly over the main core node consoles."",
                ""baseDC"": 10,
                ""isMonitored"": false,
                ""connections"": [""lobby_vents"", ""mainframe_room"", ""maintenance_room_vents""]
              },
              {
                ""roomId"": ""maintenance_room_vents"",
                ""roomName"": ""Maintenance Room Vents"",
                ""description"": ""A humid shaft dropping into the facility maintenance areas."",
                ""baseDC"": 10,
                ""isMonitored"": false,
                ""connections"": [""maintenance_room"", ""rooftop_access_vents"", ""lobby_vents"", ""mainframe_room_vents""]
              },
              {
                ""roomId"": ""maintenance_room"",
                ""roomName"": ""Maintenance Room"",
                ""description"": ""A damp facility room prone to leaking sewage and water systems."",
                ""baseDC"": 10,
                ""isMonitored"": false,
                ""connections"": [""power_room""]
              },
              {
                ""roomId"": ""rooftop_access_vents"",
                ""roomName"": ""Rooftop Access Vents"",
                ""description"": ""A drafty, freezing metal shaft climbing steeply towards the rooftop structural layout."",
                ""baseDC"": 12,
                ""isMonitored"": false,
                ""connections"": [""maintenance_room_vents"", ""rooftop_helipad"", ""rooftop_access_corridor""]
              },
              {
                ""roomId"": ""rooftop_access_corridor"",
                ""roomName"": ""Rooftop Access Corridor"",
                ""description"": ""A cold, structural corridor leading to the heavy rooftop bulkhead door. Wind whistles through the concrete seams."",
                ""baseDC"": 12,
                ""isMonitored"": true,
                ""connections"": [""rooftop_helipad"", ""rooftop_access_vents"", ""employee_hallway_l3""]
              },
              {
                ""roomId"": ""rooftop_helipad"",
                ""roomName"": ""Rooftop Helipad"",
                ""description"": ""An exposed concrete landing pad overlooking the rain-slicked towers of Butuan City. Rain slashes across the illuminated yellow circles."",
                ""baseDC"": 15,
                ""isMonitored"": true,
                ""connections"": [""rooftop_access_corridor"", ""rooftop_access_vents""]
              }
            ]";
        }
    }
}