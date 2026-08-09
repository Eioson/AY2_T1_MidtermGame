namespace Main.Models
{
    public enum DetectionState
    {
        Undetected,
        Suspicious,
        Detected,
        In_Encounter
    }

    public class Player
    {
        public string Name { get; set; }
        public int CurrentHP { get; set; } = 20;
        public int MaxHP { get; set; } = 20;

        // Base Stat Modifiers
        public int HackPTS { get; set; } = 3;
        public int SneakPTS { get; set; } = 2;
        public int FightPTS { get; set; } = 1;

        public DetectionState CurrentState { get; set; } = DetectionState.Undetected;

        public Player(string name)
        {
            Name = name;
        }
    }
}