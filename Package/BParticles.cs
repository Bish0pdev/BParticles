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
        public float Lifespan; //In Seconds
        public float Scale;
        public Texture2D Texture;
    }

    public class ParticleSystem
    {
        private List<Particle> particles;
        public  float timeScale =1.0f;
        public Texture2D ParticleTexture { get; set; }

        public Vector2 SystemPosition { get; set; }
        public bool IsLocal { get; set; }

        public bool SpawnParticles { get; set; }

        public float SpawnRate { get; set; } = 0.1f; // Default spawn rate in seconds
        private float elapsedSpawnTime = 0;


        // Delegate type for functions that modify particle attributes over time
        /// <summary>
        /// Delegate representing a function that modifies particle attributes over time.
        /// </summary>
        /// <param name="particle">The particle to be modified.</param>
        /// <param name="elapsedSeconds">The elapsed time, in seconds, since the last modification.</param>
        public delegate void ParticleAttributeModifier(Particle particle, float elapsedSeconds);

        /// <summary>
        /// List of particle attribute modifiers that control particle attributes over time.
        /// </summary>
        private List<ParticleAttributeModifier> attributeModifiers = new List<ParticleAttributeModifier>();

        /// <summary>
        /// Adds an attribute modifier function to the list of particle attribute modifiers.
        /// </summary>
        /// <param name="modifier">The attribute modifier function to be added.</param>
        public void AddAttributeModifier(ParticleAttributeModifier modifier)
        {
            attributeModifiers.Add(modifier);
        }

        /// <summary>
        /// Constructs a new ParticleSystem with the specified texture and adds default attribute modifiers.
        /// </summary>
        /// <param name="texture">The texture used for particles in the system.</param>
        public ParticleSystem(Texture2D texture)
        {
            particles = new List<Particle>();
            ParticleTexture = texture;

            SystemPosition = Vector2.Zero; // Set default system position
            IsLocal = true; // Set default isLocal value

            AddAttributeModifier(UpdateLifetime);
            AddAttributeModifier(UpdatePosition);
        }

        /// <summary>
        /// Updates all particles in the system based on their attribute modifiers.
        /// </summary>
        /// <param name="gameTime">Snapshot of the game's timing state.</param>
        public void Update(GameTime gameTime)
        {
            if (SpawnParticles)
            {
                // Calculate elapsed time since the last update
                elapsedSpawnTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Spawn particles based on the spawn rate
                while (elapsedSpawnTime > SpawnRate)
                {
                    AddParticle();
                    elapsedSpawnTime -= SpawnRate;
                }
            }

            for (int i = 0; i < particles.Count; i++)
            {
                Particle particle = particles[i];

                foreach (var modifier in attributeModifiers)
                {
                    modifier(particle, (float)gameTime.ElapsedGameTime.TotalSeconds);
                }
            }
        }

        /// <summary>
        /// Draws all particles in the system using the specified SpriteBatch.
        /// </summary>
        /// <param name="spriteBatch">The SpriteBatch used for drawing particles.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var particle in particles)
            {
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

        /// <summary>
        /// Adds a particle to the system with the specified attributes.
        /// </summary>
        /// <param name="position">The initial position of the particle.</param>
        /// <param name="velocity">The initial velocity of the particle.</param>
        /// <param name="color">The color of the particle.</param>
        /// <param name="lifespan">The lifespan of the particle in seconds.</param>
        /// <param name="scale">The scale of the particle.</param>
        public void AddParticle(Vector2 position, Vector2 velocity, Color color, float lifespan, float scale)
        {
            Particle particle = new Particle
            {
                Position = position,
                Velocity = velocity,
                Color = color,
                Lifespan = lifespan,
                Scale = scale,
                Texture = ParticleTexture
            };

            particles.Add(particle);
        }

        /// <summary>
        /// Adds a particle to the system with the specified position and velocity, using default color, lifespan, and scale.
        /// </summary>
        /// <param name="position">The initial position of the particle.</param>
        /// <param name="velocity">The initial velocity of the particle.</param>
        public void AddParticle(Vector2 position, Vector2 velocity)
        {
            AddParticle(position, velocity, Color.White, 1, 1);
        }
        /// <summary>
        /// Adds a particle to the system with the default values, at the origin of the system
        /// </summary>
        public void AddParticle()
        {
            AddParticle(SystemPosition, Vector2.Zero);
        }
        /// <summary>
        /// Allows the system to spawn particles on its own
        /// </summary>
        public void Play() { SpawnParticles=true; }

        /// <summary>
        /// Stop's the system from spawning particles on its own, you can still spawn particles manually using the AddParticle() function
        /// </summary>
        public void Stop() { SpawnParticles=false; }

        /// <summary>
        /// Updates the lifetime of a particle based on the elapsed time and the current time scale.
        /// </summary>
        /// <param name="particle">The particle whose lifetime needs to be updated.</param>
        /// <param name="elapsedSeconds">The elapsed time, in seconds, since the last update.</param>
        /// <remarks>
        /// The particle's lifespan is reduced by the product of the elapsed time and the current time scale.
        /// If the particle's remaining lifespan becomes non-positive, it is removed from the particle system.
        /// </remarks>
        protected virtual void UpdateLifetime(Particle particle, float elapsedSeconds)
        {
            particle.Lifespan -= elapsedSeconds * timeScale;

            if (particle.Lifespan <= 0)
            {
                particles.Remove(particle);
            }
        }

        /// <summary>
        /// Updates the position of a particle based on its velocity and the elapsed time.
        /// </summary>
        /// <param name="particle">The particle whose position needs to be updated.</param>
        /// <param name="elapsedSeconds">The elapsed time, in seconds, since the last update.</param>
        /// <remarks>
        /// The particle's position is updated by adding the product of its velocity and the elapsed time.
        /// </remarks>
        protected virtual void UpdatePosition(Particle particle, float elapsedSeconds)
        {
            if (IsLocal)
            {
                // If IsLocal is true, update the particle position based on SystemPosition
                Vector2 positionoffset = particle.Position - SystemPosition;
                particle.Position = (particle.Position + particle.Velocity * elapsedSeconds) + positionoffset;
            }
            else
            {
                // If IsLocal is false, update the particle position independently
                particle.Position += particle.Velocity * elapsedSeconds;
            }
        }
    }
}