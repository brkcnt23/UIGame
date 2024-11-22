using UnityEngine;

namespace DICE
{
    public class Dice
    {
        public static int Roll(int sides)
        {
            return Random.Range(1, sides + 1);
        }

        public static int RollD100()
        {
            return Roll(100);
        }

        public static int RollD20()
        {
            return Roll(20);
        }

        public static int RollD12()
        {
            return Roll(12);
        }

        public static int RollD10()
        {
            return Roll(10);
        }

        public static int RollD8()
        {
            return Roll(8);
        }

        public static int RollD6()
        {
            return Roll(6);
        }

        public static int RollD4()
        {
            return Roll(4);
        }

        public static bool RollSuccess(int targetNumber, int roll)
        {
            return roll >= targetNumber;
        }

        public static bool RollSuccess(int targetNumber, int roll, int modifier)
        {
            return roll + modifier >= targetNumber;
        }

        public static bool RollSuccess(int targetNumber, int roll, int modifier, int difficulty)
        {
            return roll + modifier >= targetNumber + difficulty;
        }

        public static bool RollCriticalSuccess(int roll)
        {
            int roll2 = RollD20();
            return roll == 20 && roll2 == 20;
        }

        public static bool RollCriticalFailure(int roll)
        {
            int roll2 = RollD20();
            return roll == 1 && roll2 == 1;
        }
    }
}