namespace CyberHeistButuan.Models
{
    public enum ItemType
    {
        LightSnack,
        HeavySnack,
        HealthKit
    }

    public class Item
    {
        public string Name { get; set; }
        public ItemType Type { get; set; }

        public Item(string name, ItemType type)
        {
            Name = name;
            Type = type;
        }
    }
}