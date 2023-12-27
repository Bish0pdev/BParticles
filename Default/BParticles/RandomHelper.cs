using System;
namespace BParticleTemplate.Default.Packages
{
    public static class RandomHelper
    {

        /// <summary>
        /// Generates a random integer between minValue (inclusive) and maxValue (exclusive).
        /// </summary>
        public static int Next(int minValue, int maxValue)
        {
            Random random = new Random();
            return random.Next(minValue, maxValue);
        }

        /// <summary>
        /// Generates a random float between 0.0 (inclusive) and 1.0 (exclusive).
        /// </summary>
        public static float NextFloat()
        {
            Random random = new Random();
            return (float)random.NextDouble();
        }

        /// <summary>
        /// Generates a random float between minValue (inclusive) and maxValue (exclusive).
        /// </summary>
        public static float NextFloat(float minValue, float maxValue)
        {
            Random random = new Random();
            return minValue + (float)random.NextDouble() * (maxValue - minValue);
        }
    }
}