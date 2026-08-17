using System.Collections.Generic;

namespace Main.Models
{
    public class Room
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        // Choice Dictionary: Choice ID -> (Display Text, Target Room, Required Stat, Target DC)
        public Dictionary<int, (string ChoiceText, Room TargetRoom, 
            string RequiredStat, int TargetDC)> Exits { get; set; }

        public Room(string id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
            Exits = new Dictionary<int, (string, Room, string, int)>();
        }
    }
}