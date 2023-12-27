using System;
namespace BParticles
{
    public class RandomHelper
    {
        private readonly Random random;

        public RandomHelper()
        {
            random = new Random();
        }

        public RandomHelper(int seed)
        {
            random = new Random(seed);
        }

        /// <summary>
        /// Generates a random integer between minValue (inclusive) and maxValue (exclusive).
        /// </summary>
        public int Next(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }

        /// <summary>
        /// Generates a random float between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        public float NextFloat()
        {
            return (float)random.NextDouble();
        }

        /// <summary>
        /// Generates a random float between minValue (inclusive) and maxValue (exclusive).
        /// </summary>
        public float NextFloat(float minValue, float maxValue)
        {
            return minValue + (float)random.NextDouble() * (maxValue - minValue);
        }

        /// <summary>
        /// Generates a random double between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        public double NextDouble()
        {
            return random.NextDouble();
        }

        /// <summary>
        /// Generates a random boolean value.
        /// </summary>
        public bool NextBoolean()
        {
            return random.Next(2) == 0;
        }
    }
}