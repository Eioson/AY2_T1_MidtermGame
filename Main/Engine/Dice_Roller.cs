using System;

namespace CyberHeistButuan.Engine
{
    public class RollResult
    {
        public int BaseRoll { get; set; }
        public int Modifier { get; set; }
        public int Total { get; set; }
        public bool IsNat20 { get; set; }
        public bool IsNat1 { get; set; }
        public bool IsSuccess { get; set; }
    }

    public static class Dice_Roller
    {
        private static readonly Random _random = new Random();

        public static RollResult RollD20(int modifier, int targetDC)
        {
            int baseRoll = _random.Next(1, 21);
            bool isNat20 = baseRoll == 20;
            bool isNat1 = baseRoll == 1;

            int total = baseRoll + modifier;
            bool isSuccess;

            if (isNat20)
            {
                isSuccess = true;
            }
            else if (isNat1)
            {
                isSuccess = false;
            }
            else
            {
                isSuccess = total >= targetDC;
            }

            return new RollResult
            {
                BaseRoll = baseRoll,
                Modifier = modifier,
                Total = total,
                IsNat20 = isNat20,
                IsNat1 = isNat1,
                IsSuccess = isSuccess
            };
        }
    }
}