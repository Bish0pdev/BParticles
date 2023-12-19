using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static System.Formats.Asn1.AsnWriter;

namespace BParticles
{
	public class Particle
	{
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Lifespan = 1; //In Seconds
        public float Scale = 1.0f;
        public Texture2D Texture;
    }

    public class ParticleSystem
    {
        private List<Particle> particles;
        public  float timeScale =1.0f;
        public Texture2D ParticleTexture { get; set; }

        public ParticleSystem(Texture2D texture)
        {
            particles = new List<Particle>();
            ParticleTexture = texture;
        }
        public ParticleSystem()
        {
            particles = new List<Particle>();
        }
        public void Update(GameTime gameTime)
        {
            List<Particle> particlesToRemove = new List<Particle>();

            for (int i = 0; i < particles.Count; i++)
            {
                Particle particle = particles[i];

                // Adjust particle lifespan based on seconds
                particle.Lifespan -= (float)gameTime.ElapsedGameTime.TotalSeconds * timeScale;


                if (particle.Lifespan <= 0)
                {
                    particlesToRemove.Add(particle);
                }
            }

            // Remove particles after iteration to avoid modification during iteration
            foreach (var particle in particlesToRemove)
            {
                particles.Remove(particle);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw all particles
            foreach (var particle in particles)
            {
                // Use the scale parameter in the spriteBatch.Draw method
                spriteBatch.Draw(
                    particle.Texture,
                    particle.Position,
                    null,
                    particle.Color,
                    0f,
                    Vector2.Zero,
                    particle.Scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }

        public void AddParticle(Vector2 position, Vector2 velocity, Color color, float lifespan, float scale, Texture2D texture)
        {
            Particle particle = new Particle
            {
                Position = position,
                Velocity = velocity,
                Color = color,
                Lifespan = lifespan,
                Scale = scale,
                Texture = texture
            };

            particles.Add(particle);
        }
    }
}