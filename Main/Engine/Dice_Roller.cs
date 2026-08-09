using System;

namespace Main.Engine
{
    public static class DiceRoller
    {
        private static readonly Random rng = new Random();

        public static (int roll, bool isNat20, bool isNat1) RollD20()
        {
            int roll = rng.Next(1, 21);
            return (roll, roll == 20, roll == 1);
        }
    }
}