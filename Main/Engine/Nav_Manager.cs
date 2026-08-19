using System;
using System.Collections.Generic;
using System.Text.Json;
using CyberHeistButuan.Models;
using CyberHeistButuan.UI;

namespace CyberHeistButuan.Engine
{
    public class RoomNode
    {
        public string RoomId { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int BaseDC { get; set; }
        public bool IsMonitored { get; set; }
        public List<string> Connections { get; set; } = new List<string>();
    }

    public class Nav_Manager
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
                    Rooms[room.RoomId] = room;
                }
            }

            // Set standard starting point
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

        // Backwards compatibility transition method
        public bool MoveTo(string roomId)
        {
            if (Rooms.TryGetValue(roomId, out var target))
            {
                CurrentRoom = target;
                return true;
            }
            return false;
        }

        // Overload to process transition checks and notifications
        public bool MoveTo(string roomId, Player player)
        {
            if (Rooms.TryGetValue(roomId, out var target))
            {
                CurrentRoom = target;
                OnEnterRoom(target, player);
                return true;
            }
            return false;
        }

        private void OnEnterRoom(RoomNode target, Player player)
        {
            if (player == null) return;

            // Trigger "Moist" status when entering Maintenance Room
            if (target.RoomId.Equals("maintenance_room", StringComparison.OrdinalIgnoreCase))
            {
                if (!player.IsMoist)
                {
                    player.IsMoist = true;
                    Terminal_Render.PrintStatusWarning(
                        "Hazardous Maintenance Zone Entered",
                        "[STATUS EFFECT] You stepped into grease, sewage, and leaking high-humidity pipe run-offs!\n" +
                        "[STATUS EFFECT] You are now MOIST.\n\n" +
                        "[MECHANICAL PENALTY] Your squelching boots make noise: Sneak Capabilities reduced (-2 SneakPTS).\n" +
                        "Warning: Entering highly electrified rooms while wet has severe electrical hazard risks!"
                    );
                }
                else
                {
                    Terminal_Render.PrintStatusWarning(
                        "Hazardous Maintenance Zone",
                        "[STATUS NOTICE] You are still sloshing through sewage. The MOIST status remains active."
                    );
                }
            }
            // Hazard penalty: Entering Power Room while Moist triggers electrical damage
            else if (target.RoomId.Equals("power_room", StringComparison.OrdinalIgnoreCase) && player.IsMoist)
            {
                player.IsMoist = false; // Arc drying effect
                double damage = 3.0;
                player.CurrentHP = Math.Max(0.0, player.CurrentHP - damage);
                
                Terminal_Render.PrintStatusWarning(
                    "High Voltage Discharge / Arc Flash!",
                    $"[HAZARD TRIGGERED] The wet film on your suit bridges the electrical gap with the generators!\n\n" +
                    $"[DAMAGE] You take {damage} points of high-voltage shock damage!\n" +
                    $"         Your current HP status is: {player.CurrentHP}/{player.MaxHP}.\n" +
                    $"         The thermal flash has dried your gear (MOIST status removed)."
                );
            }
        }

        public bool RequiresSneakCheck(RoomNode target)
        {
            // Zone-Based Approach: Patrolled / High-Security Areas require D20 check
            return target.IsMonitored;
        }
    }
}