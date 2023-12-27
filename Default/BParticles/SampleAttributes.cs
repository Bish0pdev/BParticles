using Microsoft.Xna.Framework;
using System;
namespace BParticleTemplate.Default.Packages
{
    public class SampleAttributes
    {
        // Sample attribute modifier: Rotates particles over time
        public static void RotateOverTime(Particle particle, float elapsedSeconds)
        {
            float rotationSpeed = 2.0f; // Adjust the rotation speed as needed
            particle.Position += Vector2.Transform(particle.Velocity, Matrix.CreateRotationZ(rotationSpeed * elapsedSeconds));
        }

        // Sample attribute modifier: Changes particle color over time
        public static void ColorChangeOverTime(Particle particle, float elapsedSeconds)
        {
            float colorChangechance = 0.2f; // Adjust the color change speed as needed
            if (RandomHelper.NextFloat(0, 1) < colorChangechance)
            {
                particle.Color = new Color(
                    (byte)(particle.Color.R + RandomHelper.Next(0, 2)),
                    (byte)(particle.Color.G + RandomHelper.Next(0, 2)),
                    (byte)(particle.Color.B + RandomHelper.Next(0, 2)),
                    particle.Color.A
                );
            }
        }

        // Sample spawn modifier: Randomizes initial position and velocity
        public static void RandomizePositionAndVelocity(Particle particle)
        {
            // Randomize position within a specified range
            particle.Position = new Vector2(
                RandomHelper.Next(0, 800), // Adjust the range as needed
                RandomHelper.Next(0, 600)
            );

            // Randomize velocity within a specified range
            particle.Velocity = new Vector2(
                RandomHelper.Next(-50, 50), // Adjust the range as needed
                RandomHelper.Next(-50, 50)
            );

            // Normalize the velocity for consistent speed
            if (particle.Velocity != Vector2.Zero)
            {
                particle.Velocity.Normalize();
            }
        }
    }
}