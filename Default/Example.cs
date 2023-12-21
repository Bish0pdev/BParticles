using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BParticles
{
    public class Example : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private ParticleSystem _particleSystem;
        private Vector2 _ScreenCenter;
        private Random random = new Random();
        float elapsedSpawnTime = 0.0f;
        SpriteFont font;
        public Example()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            IsFixedTimeStep = false;
            _graphics.SynchronizeWithVerticalRetrace = false;
            TargetElapsedTime = TimeSpan.FromMilliseconds(1.0 / 1000.0);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _ScreenCenter = new Vector2(Window.ClientBounds.Width/2,Window.ClientBounds.Height/2);
            Window.Title = "BParticles Example";
            Window.AllowUserResizing = false;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Texture2D squareTexture = Content.Load<Texture2D>("square");
            Texture2D animsquareTexture = Content.Load<Texture2D>("animsquare");
            font = Content.Load<SpriteFont>("Holofont");

            
            //Example particle system, Feel free to play with this
            _particleSystem = new ParticleSystem(animsquareTexture, 2, 0.5f);
            _particleSystem.AddSpawnModifier(x => x.Scale = 10f);
            _particleSystem.AddSpawnModifier(RandomColor);
            _particleSystem.AddSpawnModifier(x => x.Velocity = GetRandomVector(-50f, 50));
            _particleSystem.AddSpawnModifier(x => x.Lifespan = 1f);
            _particleSystem.SystemPosition = _ScreenCenter;
            _particleSystem.Play();

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _particleSystem.Update(gameTime);
            
            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            _particleSystem.Draw(_spriteBatch);
            _spriteBatch.End();

            _spriteBatch.Begin();
            // FPS counter
            _spriteBatch.DrawString(
                font,
                $"FPS: {1f / gameTime.ElapsedGameTime.TotalSeconds:0}",
                new Vector2(10, 10),
                Color.White
            );

            // Active Particles counter
            _spriteBatch.DrawString(
                font,
                $"Active Particles: {_particleSystem.particles.Count:0}",
                new Vector2(10, 30),
                Color.White
            );

            // Draw Calls counter
            _spriteBatch.DrawString(
                font,
                $"Draw Calls: {GraphicsDevice.Metrics.DrawCount}",
                new Vector2(10, 50),
                Color.White
            );
            //
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        #region Example Particle Modifiers

        public void RandomColor(Particle particle)
        {
            particle.Color = new Color(
            (float)random.NextDouble(), // Red component
            (float)random.NextDouble(), // Green component
            (float)random.NextDouble(), // Blue component
            1.0f                         // Alpha component (fully opaque)
            );
        }
        #endregion

        #region Manual Spawning Examples

        private void RandomSpawns(float spawnInterval)
        {
            while (elapsedSpawnTime > spawnInterval)
            {
                // Randomize position
                float x = (float)random.NextDouble() * GraphicsDevice.Viewport.Width;
                float y = (float)random.NextDouble() * GraphicsDevice.Viewport.Height;

                // Randomize velocity
                float vx = (float)(random.NextDouble() * 2 - 1); // Random value between -1 and 1
                float vy = (float)(random.NextDouble() * 2 - 1); // Random value between -1 and 1
                Vector2 velocity = new Vector2(vx * 10, vy * 10);

                // Randomize color
                Color color = new Color(
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble()
                );

                // Randomize lifespan
                float lifespan = (float)random.NextDouble() * 2 + 1; // Random value between 1 and 3 seconds

                // Randomize scale
                float scale = (float)random.NextDouble() * 0.5f + 0.5f; // Random value between 0.5 and 1.0

                // Add the new particle
                _particleSystem.AddParticle(new Vector2(x, y), velocity, color, lifespan, scale);

                elapsedSpawnTime -= spawnInterval; // Subtract the spawn interval to account for the spawn
            }
        }
        #endregion

        public static Vector2 GetRandomVector(float minValue, float maxValue)
        {
            Random random = new Random();
            float x = (float)(random.NextDouble() * (maxValue - minValue) + minValue);
            float y = (float)(random.NextDouble() * (maxValue - minValue) + minValue);
            return new Vector2(x, y);
        }
    }

}
