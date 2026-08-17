using System;
using System.Collections.Generic;
using System.Text.Json;
using CyberHeistAce.Models;
using CyberHeistButuan.Engine;

namespace CyberHeistAce.Engine
{
    public class NavigationManager
    {
        public Dictionary<string, RoomNode> Rooms { get; private set; } = new Dictionary<string, RoomNode>();
        public RoomNode CurrentRoom { get; private set; } = new RoomNode();

        public void LoadMap(string jsonContent)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var list = JsonSerializer.Deserialize<List<RoomNode>>(jsonContent, options);

            if (list != null)
            {
                foreach (var room in list)
                {
                    if (room.RoomId.EndsWith("_vents"))
                    {
                        room.IsVent = true;
                        room.CorrespondingRoomId = room.RoomId.Replace("_vents", "");
                    }

                    // Manually tag hazards to match backend environment specs
                    if (room.RoomId == "maintenance_room")
                    {
                        room.HazardType = "Moist";
                    }

                    Rooms[room.RoomId] = room;
                }
            }

            if (Rooms.ContainsKey("outside"))
            {
                CurrentRoom = Rooms["outside"];
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

        /// <summary>
        /// Evaluates, applies, and notifies the player of entry hazards or environmental clearances.
        /// </summary>
        public void HandleRoomEntryHazards(Player player)
        {
            // Evaluate 'Moist' (Squeaky Shoes) status from damp floors
            if (CurrentRoom.HazardType == "Moist")
            {
                if (!player.IsMoist)
                {
                    player.IsMoist = true;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[HAZARD WARNING] The damp floor of the Maintenance Room drenches your shoes! Your steps squeak loudly. Stealth debuffed (-1 sneakPTS).");
                    Console.ResetColor();
                }
            }
            else
            {
                // Clear the squeak effect when leaving wet zones
                if (player.IsMoist)
                {
                    player.IsMoist = false;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[HAZARD CLEARED] Your shoes dry up. Stealth penalty removed (+1 sneakPTS restored).");
                    Console.ResetColor();
                }
            }

            // Evaluate 'Hot' status
            if (CurrentRoom.HazardType == "Hot")
            {
                if (!player.IsHot)
                {
                    player.IsHot = true;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n[HAZARD WARNING] Escaping high-pressure steam rises your body temperature! Hot status active (HP will drain gradually).");
                    Console.ResetColor();
                }
            }
            else
            {
                if (player.IsHot)
                {
                    player.IsHot = false;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[HAZARD CLEARED] You step out of the hot zone. Thermal drain stopped.");
                    Console.ResetColor();
                }
            }
        }

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

        public void WaitInVents(Player player, Detection_System detectionSystem)
        {
            Console.WriteLine("\n[ACTION] You find a dark corner inside the vent shaft to lay low...");
            detectionSystem.ProcessTurn();

            // Run check to stay silent inside ducts
            var checkResult = Dice_Roller.RollD20(player.EffectiveSneakPTS, 10);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[VENT COOLDOWN CHECK] (DC 10): D20 + {player.EffectiveSneakPTS} (Base Roll: {checkResult.BaseRoll}) = {checkResult.Total}");
            Console.ResetColor();

            if (checkResult.IsSuccess)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("SUCCESS! You lay low quietly. Local security searches cool down.");
                Console.ResetColor();

                if (detectionSystem.CurrentState == DetectionState.Suspicious)
                {
                    detectionSystem.ModifySuspiciousTimer(-1);
                    Console.WriteLine($"Remaining Suspicious Turns: {detectionSystem.GetSuspiciousTurnsRemaining()}");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[FAILURE] You shift your weight and slide against a loose bracket! Clank!");
                Console.ResetColor();

                if (detectionSystem.CurrentState == DetectionState.Suspicious)
                {
                    detectionSystem.ModifySuspiciousTimer(1);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("ALERT: Local sound waves delayed your cooldown! Suspicious timer increased +1.");
                    Console.WriteLine($"Remaining Suspicious Turns: {detectionSystem.GetSuspiciousTurnsRemaining()}");
                    Console.ResetColor();
                }
                else if (detectionSystem.CurrentState == DetectionState.Undetected)
                {
                    detectionSystem.SetState(DetectionState.Suspicious);
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("ALERT: Sound echoes through the ducts! Security status set to SUSPICIOUS.");
                    Console.ResetColor();
                }
            }
        }
    }
}