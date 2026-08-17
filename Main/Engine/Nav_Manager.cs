using System;
using System.Collections.Generic;
using System.Text.Json;
using CyberHeistButuan.Models;

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
            // Zone-Based Approach: Patrolled / High-Security Areas require D20 check
            return target.IsMonitored;
        }
    }
}