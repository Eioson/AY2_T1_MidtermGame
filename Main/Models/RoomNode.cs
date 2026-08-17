using System.Collections.Generic;

namespace CyberHeistAce.Models
{
    public class RoomNode
    {
        public string RoomId { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int BaseDC { get; set; }
        public bool IsMonitored { get; set; }
        public List<string> Connections { get; set; } = new List<string>();

        // Vent drop-down attributes
        public bool IsVent { get; set; }
        public string CorrespondingRoomId { get; set; } = string.Empty;

        // Environmental Hazard tracking
        public string HazardType { get; set; } = string.Empty;
    }
}