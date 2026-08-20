using UnityEngine;

namespace NEXUS.Utilities
{
    public class Dice
    {
        public static int Roll(int sides)
        {
            return Random.Range(1, sides + 1);
        }
        public static int Roll(int min, int max)
        {
            return Random.Range(min, max);
        }

        /// <summary>
        /// A random list index in the range 0..count-1, or -1 when the list is empty.
        ///
        /// Use this instead of Roll(list.Count). Roll is a die: it returns 1..N and
        /// never 0, so using it as an index is off by one and throws on the last
        /// element — or on any single-item list.
        /// </summary>
        public static int Index(int count)
        {
            return count <= 0 ? -1 : Random.Range(0, count);
        }

        /// <summary>Random element from a list, or default when empty.</summary>
        public static T Pick<T>(System.Collections.Generic.IList<T> list)
        {
            if (list == null || list.Count == 0)
                return default;

            return list[Random.Range(0, list.Count)];
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
        public static bool RollSuccessWithBonus(int targetNumber, int roll, int modifier, int difficulty)
        {
            return roll + modifier >= difficulty - targetNumber;
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