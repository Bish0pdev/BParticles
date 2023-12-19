using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace BParticles
{
    public class TestingScene : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private ParticleSystem _particleSystem;
        private Vector2 _ScreenCenter;
        private Random random = new Random();
        public TestingScene()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _ScreenCenter = new Vector2(Window.ClientBounds.Width/2,Window.ClientBounds.Height/2);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Texture2D particleTexture = Content.Load<Texture2D>("square");

            _particleSystem = new ParticleSystem(particleTexture);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //--Sample particle system--

            float spawnChance = 0.1f; // Adjust this value based on the desired spawn rate

            if (random.NextDouble() < spawnChance)
            {
                // Randomize position within the screen bounds
                float x = (float)random.NextDouble() * GraphicsDevice.Viewport.Width;
                float y = (float)random.NextDouble() * GraphicsDevice.Viewport.Height;

                // Randomize velocity (optional)
                float vx = (float)(random.NextDouble() * 2 - 1); // Random value between -1 and 1
                float vy = (float)(random.NextDouble() * 2 - 1); // Random value between -1 and 1
                Vector2 velocity = new Vector2(vx*10, vy*10);

                // Randomize color (optional)
                Color color = new Color(
                    (float)random.NextDouble(),
                    (float)random.NextDouble(),
                    (float)random.NextDouble()
                );

                // Randomize lifespan (optional)
                float lifespan = (float)random.NextDouble() * 2 + 1; // Random value between 1 and 3 seconds

                // Randomize scale (optional)
                float scale = (float)random.NextDouble() * 0.5f + 0.5f; // Random value between 0.5 and 1.0

                // Add the new particle to the particle system
                _particleSystem.AddParticle(new Vector2(x, y), velocity, color, lifespan, scale, _particleSystem.ParticleTexture);
            }

            _particleSystem.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            _particleSystem.Draw(_spriteBatch);
            _spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
