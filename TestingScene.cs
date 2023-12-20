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
        float elapsedSpawnTime = 0.0f;
        SpriteFont font;
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
            font = Content.Load<SpriteFont>("Holofont");
            _particleSystem = new ParticleSystem(particleTexture);

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            //--Sample particle system--

            float spawnRatePerSecond = 10.0f; // Adjust this value based on the desired spawn rate per second
            float spawnInterval = 1.0f / spawnRatePerSecond; // Calculate the time interval between spawns

            elapsedSpawnTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

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
                _particleSystem.AddParticle(new Vector2(x, y), velocity, color, lifespan, scale, _particleSystem.ParticleTexture);

                elapsedSpawnTime -= spawnInterval; // Subtract the spawn interval to account for the spawn
            }

            _particleSystem.Update(gameTime);

            base.Update(gameTime);
        }

        

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            _particleSystem.Draw(_spriteBatch);

            _spriteBatch.DrawString(
            // Use your preferred SpriteFont and color
            font,
            $"FPS: {1f / gameTime.ElapsedGameTime.TotalSeconds:0}",
            new Vector2(GraphicsDevice.Viewport.Width - 100, 10),
            Color.White
            );
            _spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
