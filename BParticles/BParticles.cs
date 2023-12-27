using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static System.Formats.Asn1.AsnWriter;
using System.Diagnostics;
namespace BParticles
{
    public class ParticleSystem
    {
        /// <summary>
        /// List of particles in the particle system.
        /// </summary>
        public List<Particle> particles;

        /// <summary>
        /// The time scale for controlling the speed of particle updates.
        /// </summary>
        public float timeScale = 1f;

        /// <summary>
        /// Gets or sets the texture used for particles in the system.
        /// </summary>
        public Texture2D ParticleTexture { get; set; }

        /// <summary>
        /// Gets or sets the position of the particle system in the game world.
        /// </summary>
        public Vector2 SystemPosition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the particle system updates particles locally or globally.
        /// </summary>
        public bool IsLocal { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the particle system should spawn particles.
        /// </summary>
        public bool SpawnParticles { get; set; }

        /// <summary>
        /// Gets or sets the spawn rate of particles in seconds.
        /// </summary>
        public float SpawnRate { get; set; } = 0.1f; // Default spawn rate in seconds

        /// <summary>
        /// Elapsed time since the last particle spawn.
        /// </summary>
        private float elapsedSpawnTime = 0;

        #region Delegates and Modifiers
        /// <summary>
        /// Delegate representing a function that modifies particle attributes during spawn.
        /// </summary>
        public delegate void ParticleSpawnModifier(Particle particle);

        /// <summary>
        /// List of particle spawn modifiers that control particle attributes during spawn.
        /// </summary>
        public List<ParticleSpawnModifier> spawnModifiers = new List<ParticleSpawnModifier>();

        /// <summary>
        /// Adds a particle spawn modifier function to the list of modifiers.
        /// </summary>
        /// <param name="modifier">The particle spawn modifier function to be added.</param>
        public void AddSpawnModifier(ParticleSpawnModifier modifier)
        {
            spawnModifiers.Add(modifier);
        }

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
        #endregion

        #region Constructors
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
        /// Constructs a new ParticleSystem with the specified animated texture and adds default attribute modifiers.
        /// </summary>
        /// <param name="animatedTexture">The animated texture used for particles in the system.</param>
        /// <param name="totalFrames">The total number of frames in the animation.</param>
        /// <param name="frameDuration">The duration of each frame in seconds.</param>
        public ParticleSystem(Texture2D animatedTexture, int totalFrames, float frameDuration)
        {
            particles = new List<Particle>();
            ParticleTexture = animatedTexture;

            SystemPosition = Vector2.Zero; // Set default system position
            IsLocal = true; // Set default isLocal value

            AddSpawnModifier(x => x.TotalFrames = totalFrames);
            AddSpawnModifier(x => x.FrameDuration = frameDuration);

            AddAttributeModifier(UpdateLifetime);
            AddAttributeModifier(UpdatePosition);
            AddAttributeModifier(Animate);
        }
        #endregion

        /// <summary>
        /// Updates all particles in the system based on their attribute modifiers.
        /// </summary>
        /// <param name="gameTime">Snapshot of the game's timing state.</param>
        public void Update(GameTime gameTime)
        {
            // Calculate elapsed time since the last update
            elapsedSpawnTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (SpawnParticles)
            {
                // Spawn particles based on the spawn rate
                while (elapsedSpawnTime > SpawnRate)
                {
                    AddParticle(spawnModifiers);
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
            // Group particles by texture
            Dictionary<Texture2D, List<Particle>> particleGroups = new Dictionary<Texture2D, List<Particle>>();

            foreach (var particle in particles)
            {
                if (!particleGroups.ContainsKey(particle.Texture))
                {
                    particleGroups[particle.Texture] = new List<Particle>();
                }

                particleGroups[particle.Texture].Add(particle);
            }

            // Draw particles in batches
            foreach (var group in particleGroups)
            {
                Texture2D texture = group.Key;
                List<Particle> particlesWithSameTexture = group.Value;

                foreach (var particle in particlesWithSameTexture)
                {
                    int frameWidth = particle.Texture.Width;
                    // Calculate the source rectangle for the current frame
                    if (particle.TotalFrames > 0)
                    {
                        frameWidth /= particle.TotalFrames;
                    }
                    Rectangle sourceRectangle = new Rectangle(
                        frameWidth * particle.CurrentFrame,
                        0,
                        frameWidth,
                        particle.Texture.Height);

                    // Calculate the origin as half of the particle texture size
                    Vector2 origin = new Vector2(frameWidth / 2f, particle.Texture.Height / 2f);
                    float normalizedAlpha = particle.Color.A / 255.0f;
                    spriteBatch.Draw(
                        particle.Texture,
                        particle.Position,
                        sourceRectangle, // Use the source rectangle for animation
                        particle.Color * normalizedAlpha,
                        0f,
                        origin,
                        particle.Scale,
                        SpriteEffects.None,
                        0f
                    );
                }
            }
        }

        #region Particle Management
        /// <summary>
        /// Adds a particle to the system with the specified attributes.
        /// </summary>
        /// <param name="position">The initial position of the particle.</param>
        /// <param name="velocity">The initial velocity of the particle.</param>
        /// <param name="color">The color of the particle.</param>
        /// <param name="lifespan">The lifespan of the particle in seconds.</param>
        /// <param name="scale">The scale of the particle.</param>
        public Particle AddParticle(Vector2 position, Vector2 velocity, Color color, float lifespan, float scale)
        {
            Particle particle = new Particle
            {
                Position = position,
                Velocity = velocity,
                Color = color,
                Lifespan = lifespan,
                Scale = scale,
                Texture = ParticleTexture,
                parentSystem = this
            };

            particles.Add(particle);
            return particle;
        }

        /// <summary>
        /// Adds a particle to the system with the specified position and velocity, using default color, lifespan, and scale.
        /// </summary>
        /// <param name="position">The initial position of the particle.</param>
        /// <param name="velocity">The initial velocity of the particle.</param>
        public Particle AddParticle(Vector2 position, Vector2 velocity)
        {
            return AddParticle(position, velocity, Color.White, 1, 1);
        }
        /// <summary>
        /// Adds a particle to the system with the default values, at the origin of the system
        /// </summary>
        public Particle AddParticle()
        {
            return AddParticle(SystemPosition, Vector2.One);
        }

        /// <summary>
        /// Adds a particle to the system with the specified modifiers.
        /// </summary>
        /// <param name="modifiers">The list of modifiers to apply to the particle.</param>
        public void AddParticle(List<ParticleSpawnModifier> modifiers)
        {
            Particle n = AddParticle();
            // Apply modifiers to the particle
            foreach (var modifier in modifiers)
            {
                modifier(n);
            }
        }

        public void ClearParticles()
        {
            particles.Clear();
        }

        #endregion

        #region Play and Stop Methods
        /// <summary>
        /// Allows the system to spawn particles on its own
        /// </summary>
        public void Play()
        {
            SpawnParticles = true;
        }
        /// <summary>
        /// Stop's the system from spawning particles on its own, you can still spawn particles manually using the AddParticle() function
        /// </summary>
        public void Stop() { SpawnParticles = false; }
        #endregion

        #region Attribute Modifiers
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
                particle.Position += particle.Velocity * elapsedSeconds;
            }
            else
            {
                particle.Position += particle.Velocity * elapsedSeconds;
            }
        }



        /// <summary>
        /// Animates the particle
        /// </summary>
        /// <param name="particleSystem"></param>
        /// <param name="elapsedSeconds"></param>
        protected static void Animate(Particle particle, float elapsedSeconds)
        {
            particle.UpdateFrame(elapsedSeconds);
        }
        #endregion
    }

    public class Particle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color Color;
        public float Lifespan; //In Seconds
        public float Scale;
        public Texture2D Texture;

        public ParticleSystem parentSystem;

        public int CurrentFrame { get; set; }
        public int TotalFrames { get; set; }
        public float FrameDuration { get; set; }
        public float FrameElapsed { get; set; }




        /// <summary>
        /// Update the frame based on elapsed time and frame duration
        /// </summary>
        /// <param name="elapsedSeconds"></param>
        public void UpdateFrame(float elapsedSeconds)
        {
            if (TotalFrames == 0) return;
            FrameElapsed += elapsedSeconds;

            if (FrameElapsed > FrameDuration)
            {
                CurrentFrame = (CurrentFrame + 1) % TotalFrames;
                FrameElapsed = 0;
            }
        }
    }

}