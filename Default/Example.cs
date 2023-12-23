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
            _particleSystem = new ParticleSystem(animsquareTexture, 2, 0.1f);
            _particleSystem.SpawnRate = 0.01f;
            _particleSystem.AddSpawnModifier(RandomColor);
            _particleSystem.AddSpawnModifier(x => x.Scale = 1f);
            _particleSystem.AddSpawnModifier(x => x.Velocity = GetRandomVector(-50f, 50));
            _particleSystem.AddSpawnModifier(x => x.Lifespan = 1f);
            _particleSystem.AddAttributeModifier(ApplyGravity);
            _particleSystem.AddAttributeModifier(BounceOffWalls);
            _particleSystem.AddAttributeModifier(SampleAttributes.ColorChangeOverTime);
            _particleSystem.AddAttributeModifier(RandomColorFRram);
            _particleSystem.SystemPosition = _ScreenCenter;
            _particleSystem.Play();

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if (Keyboard.GetState().IsKeyDown(Keys.F1))
            {
                // Destroy all active particles
                _particleSystem.ClearParticles();
            }
            _particleSystem.Update(gameTime);
            
            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            null);
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

        #region Particle Modifiers

        public void RandomColor(Particle particle)
        {
            particle.Color = new Color(
            (float)random.NextDouble(), // Red component
            (float)random.NextDouble(), // Green component
            (float)random.NextDouble(), // Blue component
            1.0f                         // Alpha component (fully opaque)
            );
        }

        public void RandomColorFRram(Particle particle, float t)
        {
            particle.Color = new Color(
            (float)random.NextDouble(), // Red component
            (float)random.NextDouble(), // Green component
            (float)random.NextDouble(), // Blue component
            1.0f                         // Alpha component (fully opaque)
            );
        }

        private const float Gravity = 100f;
        // Particle attribute modifier for applying gravity
        public void ApplyGravity(Particle particle, float elapsedSeconds)
        {
            // Gravity formula: acceleration = gravity constant * elapsed time
            float acceleration = Gravity * elapsedSeconds;

            // Apply the gravitational force in the downward direction (assuming positive Y is downward)
            particle.Velocity += new Vector2(0, acceleration);
        }
        public void BounceOffWalls(Particle particle, float elapsedSeconds)
        {
            Vector2 screenSize = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height); // Adjust as needed
            float damping = 1f;
            if (particle.Position.X < 0 || particle.Position.X > screenSize.X)
            {
                particle.Velocity.X *= -1 / damping;
            }
            if (particle.Position.Y < 0 || particle.Position.Y > screenSize.Y)
            {
                particle.Velocity.Y *= -1 / damping;
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
