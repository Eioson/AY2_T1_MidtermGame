using System;
using System.Collections.Generic;
using System.Text.Json;
using CyberHeistAce.Models;
using CyberHeistButuan.Models;
using CyberHeistAce.UI;

namespace CyberHeistButuan.Engine
{
    public class NavigationManager
    {
        public Dictionary<string, RoomNode> Rooms { get; private set; } = new Dictionary<string, RoomNode>();
        public RoomNode CurrentRoom { get; private set; } = new RoomNode();
        public bool IsGameOver { get; private set; } = false;

        /// <summary>
        /// Decoupled DTO class to safely map Map.json property keys to the RoomNode domain model.
        /// </summary>
        private class RoomNodeDTO
        {
            public string RoomID { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string SecurityZone { get; set; } = string.Empty;
            public bool IsVent { get; set; }
            public string CorrespondingRoomID { get; set; } = string.Empty;
            public string HazardType { get; set; } = string.Empty;
            public int BaseDC { get; set; }
            public List<string> ConnectedRoomIDs { get; set; } = new List<string>();
        }

        public void LoadMap(string jsonContent)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dtoList = JsonSerializer.Deserialize<List<RoomNodeDTO>>(jsonContent, options);

            Rooms.Clear();

            if (dtoList != null)
            {
                foreach (var dto in dtoList)
                {
                    var domainNode = new RoomNode
                    {
                        RoomId = dto.RoomID,
                        RoomName = dto.Name,
                        Description = dto.Description,
                        BaseDC = dto.BaseDC,
                        IsMonitored = dto.SecurityZone == "Monitored" || dto.SecurityZone == "Patrolled",
                        IsVent = dto.IsVent,
                        CorrespondingRoomId = dto.CorrespondingRoomID,
                        HazardType = dto.HazardType,
                        Connections = dto.ConnectedRoomIDs ?? new List<string>()
                    };

                    // Auto-wire default drop-down properties based on naming convention
                    if (domainNode.RoomId.EndsWith("_vents"))
                    {
                        domainNode.IsVent = true;
                        domainNode.CorrespondingRoomId = domainNode.RoomId.Replace("_vents", "");
                    }

                    Rooms[domainNode.RoomId] = domainNode;
                }
            }

            EnsureRequiredPathways();

            if (Rooms.ContainsKey("outside"))
            {
                CurrentRoom = Rooms["outside"];
            }
        }

        /// <summary>
        /// Automatically checks for and wires the required Ground Exit pathway.
        /// </summary>
        private void EnsureRequiredPathways()
        {
            // Register Outside Exit if not present in the map file
            if (!Rooms.ContainsKey("outside_exit"))
            {
                Rooms["outside_exit"] = new RoomNode
                {
                    RoomId = "outside_exit",
                    RoomName = "Outside Exit",
                    Description = "The hospital perimeter gates. Beyond lies safety and freedom from security sweeps.",
                    IsMonitored = false,
                    IsVent = false,
                    BaseDC = 10,
                    Connections = new List<string> { "lobby" }
                };
            }

            // Wire Lobby -> Outside Exit connection
            if (Rooms.TryGetValue("lobby", out var lobby))
            {
                if (!lobby.Connections.Contains("outside_exit"))
                {
                    lobby.Connections.Add("outside_exit");
                }
            }
        }

        public List<RoomNode> GetConnections()
        {
            var connections = new List<RoomNode>();
            if (CurrentRoom == null) return connections;

            foreach (var connId in CurrentRoom.Connections)
            {
                if (Rooms.TryGetValue(connId, out var room))
                {
                    connections.Add(room);
                }
            }
            return connections;
        }

        public bool MoveTo(string roomId)
        {
            if (Rooms.TryGetValue(roomId, out var target))
            {
                CurrentRoom = target;
                return true;
            }
            return false;
        }

        public bool RequiresSneakCheck(RoomNode target)
        {
            return target.IsMonitored;
        }

        // ==========================================
        // Vent Interaction Mechanics
        // ==========================================

        public bool CanDropDown(out string targetRoomName)
        {
            targetRoomName = string.Empty;
            if (CurrentRoom.IsVent && !string.IsNullOrEmpty(CurrentRoom.CorrespondingRoomId))
            {
                if (Rooms.TryGetValue(CurrentRoom.CorrespondingRoomId, out var targetRoom))
                {
                    targetRoomName = targetRoom.RoomName;
                    return true;
                }
            }
            return false;
        }

        public bool ExecuteDropDown()
        {
            if (CanDropDown(out _))
            {
                string targetId = CurrentRoom.CorrespondingRoomId;
                MoveTo(targetId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles the dedicated navigation loop options when inside a Vent.
        /// </summary>
        public void HandleVentMenuLoop(Player player, Detection_System detectionSystem)
        {
            TerminalRenderer.PrintAmbient("\n=== VENT ACTION INTERFACE ===");
            TerminalRenderer.PrintAmbient($"Location: {CurrentRoom.RoomName}");

            string dropDownLabel = "Drop down into room below";
            if (CanDropDown(out string targetName))
            {
                dropDownLabel = $"Drop down into [{targetName}]";
            }

            Console.WriteLine($" [1] {dropDownLabel}");
            Console.WriteLine(" [2] Lay Low / Wait inside Vents (Passes 1 turn, cools down alert level)");

            Console.Write("\nSelect action: ");
            string choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    if (ExecuteDropDown())
                    {
                        TerminalRenderer.PrintSuccess($"\nYou dropped down out of the vents and entered: {CurrentRoom.RoomName}");
                    }
                    else
                    {
                        Console.WriteLine("There is no valid corresponding room below this ventilation duct.");
                    }
                    break;
                case "2":
                    WaitInVents(player, detectionSystem);
                    break;
                default:
                    Console.WriteLine("Invalid input. Selection aborted.");
                    break;
            }
        }

        public void WaitInVents(Player player, Detection_System detectionSystem)
        {
            TerminalRenderer.PrintAmbient("\nYou find a quiet alcove in the metal vents and lay low...");
            detectionSystem.ProcessTurn();

            // DC 10 Stealth check using player EffectiveSneakPTS
            var checkResult = Dice_Roller.RollD20(player.EffectiveSneakPTS, 10);
            TerminalRenderer.PrintRoll($"Stealth Cooldown Check (DC 10): D20 + {player.EffectiveSneakPTS} (Base Roll: {checkResult.BaseRoll}) = {checkResult.Total}");

            if (checkResult.IsSuccess)
            {
                TerminalRenderer.PrintSuccess("SUCCESS! You remain dead quiet. Local security presence cools down.");
                if (detectionSystem.CurrentState == DetectionState.Suspicious)
                {
                    detectionSystem.ModifySuspiciousTimer(-1);
                    Console.WriteLine($"Alert timer lowered. Turns remaining: {detectionSystem.GetSuspiciousTurnsRemaining()}");
                }
            }
            else
            {
                TerminalRenderer.PrintAlarm("\nFAILURE! You brush against a loose metallic fitting! Clank!");
                if (detectionSystem.CurrentState == DetectionState.Suspicious)
                {
                    detectionSystem.ModifySuspiciousTimer(1);
                    TerminalRenderer.PrintAlarm($"Local security sweeps extend! Suspicious timer increased. Turns remaining: {detectionSystem.GetSuspiciousTurnsRemaining()}");
                }
                else if (detectionSystem.CurrentState == DetectionState.Undetected)
                {
                    detectionSystem.SetState(DetectionState.Suspicious);
                    TerminalRenderer.PrintAlarm("ALERT: Patrols heard a sound! Security alert level escalated to SUSPICIOUS.");
                }
            }
        }

        // ==========================================
        // Ground Win Condition Hook
        // ==========================================

        /// <summary>
        /// Checks if the player has entered the ground escape zone.
        /// </summary>
        public void CheckGroundEscapeTrigger(Player player, Detection_System detectionSystem, int totalTurns)
        {
            if (CurrentRoom.RoomId == "outside_exit")
            {
                ExecuteGroundEscape(player, detectionSystem, totalTurns);
            }
        }

        private void ExecuteGroundEscape(Player player, Detection_System detectionSystem, int totalTurns)
        {
            IsGameOver = true;

            TerminalRenderer.PrintSuccess("\n=======================================================");
            TerminalRenderer.PrintSuccess("           GROUND LEVEL EXFILTRATION SUCCESS           ");
            TerminalRenderer.PrintSuccess("=======================================================");
            TerminalRenderer.PrintAmbient("You slip past the perimeter security gate of ACE Hospital, dissolving into the rain.");
            TerminalRenderer.PrintAmbient("Before alarms lock down the surrounding blocks, you vanish into the streets.");

            TerminalRenderer.PrintSuccess("\n--- CYBER HEIST ACE MISSION SUMMARY ---");
            TerminalRenderer.PrintAmbient($"Hacker Name: {player.Name}");
            TerminalRenderer.PrintAmbient($"Exfiltration Route: Ground Level Escape Corridor");
            TerminalRenderer.PrintAmbient($"Total Turns Spent: {totalTurns}");
            
            double damageSustained = player.MaxHP - player.CurrentHP;
            TerminalRenderer.PrintAmbient($"Vitals Lost: {damageSustained} HP");
            
            TerminalRenderer.PrintAmbient("\nYou successfully escape into the dark alleys with the hospital's private medical secrets.");
            TerminalRenderer.PrintSuccess("----------------------------------------");
            
            TerminalRenderer.PrintAmbient("\nPress any key to exit the system simulation...");
            Console.ReadKey(true);
        }
    }
}