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

        private ParticleSystem Snow;
        private ParticleSystem SnowballTemplate;
        private Vector2 _ScreenCenter;
        private Random random = new Random();
        float elapsedSpawnTime = 0.0f;

        private int floor_height = 100;

        SpriteFont font;
        public Vector2 initialdir = Vector2.Zero;

        Texture2D pixelTexture;

        private ParticleSystem heldsnowball;
        Vector2 mouseVelocity = Vector2.Zero;
        float SnowballRadius = 2;
        float SnowballDensity = 12;
        MouseState mouseState;
        bool rmousebutton, lmousebutton;
        List<Particle> particlesinradius = new List<Particle>();
        private List<ParticleSystem> activeSnowballs = new List<ParticleSystem>();
        private Vector2 previousMousePosition = Vector2.Zero;

        public Example()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
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
            pixelTexture = Content.Load<Texture2D>("square");
            Texture2D snowballtexture = Content.Load<Texture2D>("circle");
            Texture2D animsquareTexture = Content.Load<Texture2D>("animsquare");
            font = Content.Load<SpriteFont>("Holofont");


            //Build the Snow particle System
            Snow = new ParticleSystem(animsquareTexture, 2, 0.1f);
            Snow.SpawnRate = 0.01f;
            Snow.AddSpawnModifier(SetSnowAttributes);
            Snow.AddAttributeModifier(AttractToMouse);
            Snow.AddAttributeModifier(ApplyGravity);
            Snow.AddAttributeModifier(BounceOffWalls);
            Snow.AddAttributeModifier(Friction);
            Snow.SystemPosition = _ScreenCenter;
            Snow.Play();

            //Build the Snowball Template
            SnowballTemplate = new ParticleSystem(snowballtexture);
            SnowballTemplate.AddSpawnModifier(x =>
            {
                x.Scale = SnowballRadius;
                x.Position = new Vector2(mouseState.X, mouseState.Y);
            });
            SnowballTemplate.AddAttributeModifier(InfiniteLifespan);
            SnowballTemplate.AddAttributeModifier(SnowballModifier);
            SnowballTemplate.SystemPosition = _ScreenCenter;
        }

        protected override void Update(GameTime gameTime)
        {
            

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            mouseState = Mouse.GetState();
            rmousebutton = mouseState.RightButton == ButtonState.Pressed;
            lmousebutton = mouseState.LeftButton == ButtonState.Pressed;

            mouseState = Mouse.GetState();
            Vector2 currentMousePosition = new Vector2(mouseState.X, mouseState.Y);

            // Calculate mouse velocity
            mouseVelocity = (currentMousePosition - previousMousePosition) / (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Store current mouse position for the next frame
            previousMousePosition = currentMousePosition;
            if (Keyboard.GetState().IsKeyDown(Keys.F1))
            {
                // Destroy all active particles
                Snow.ClearParticles();
                foreach (var snowball in activeSnowballs)
                {
                    snowball.ClearParticles();
                }
                activeSnowballs.Clear();
            }
            Snow.Update(gameTime);
            particlesinradius = Snow.ParticlesInRadius(new Vector2(mouseState.X, mouseState.Y), SnowballRadius);
            if (particlesinradius.Count>=SnowballDensity && heldsnowball == null)
            {
                if (particlesinradius.Count >= SnowballDensity && heldsnowball == null)
                {
                    // Create a single snowball system
                    heldsnowball = SnowballTemplate.Duplicate();
                    heldsnowball.AddSpawnModifier(x => x.Position = new Vector2(mouseState.X, mouseState.Y));
                    heldsnowball.AddParticle(heldsnowball.spawnModifiers);
                    activeSnowballs.Add(heldsnowball);
                    heldsnowball.AddAttributeModifier(HoldParticle);
                    // Remove particles in the radius from the Snow system
                    foreach (var particle in particlesinradius)
                    {
                        Snow.particles.Remove(particle);
                    }
                }
            }
            foreach (var snowball in activeSnowballs)
            {
                snowball.Update(gameTime);
            }
            activeSnowballs.RemoveAll(snowball => snowball.particles.Count == 0);
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

            _spriteBatch.Draw(
            pixelTexture,
            new Rectangle(0, GraphicsDevice.Viewport.Height - floor_height, GraphicsDevice.Viewport.Width, floor_height),
            Color.Green
            );

            Snow.Draw(_spriteBatch);

            // Draw active snowballs
            foreach (var snowball in activeSnowballs)
            {
                snowball.Draw(_spriteBatch);
            }
            _spriteBatch.End();

            _spriteBatch.Begin();
            // FPS counter
            _spriteBatch.DrawString(
                font,
                $"FPS: {1f / gameTime.ElapsedGameTime.TotalSeconds:0}",
                new Vector2(10, 10),
                Color.White
            );

            // Draw Calls counter
            _spriteBatch.DrawString(
                font,
                $"Draw Calls: {GraphicsDevice.Metrics.DrawCount}",
                new Vector2(10, 30),
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

        private const float Gravity = 100f;
        public void ApplyGravity(Particle particle, float elapsedSeconds)
        {
            // Gravity formula: acceleration = gravity constant * elapsed time
            float acceleration = Gravity * elapsedSeconds;

            // Apply the gravitational force in the downward direction (assuming positive Y is downward)
            particle.Velocity += new Vector2(0, acceleration);
        }

        public void ChangeSize(Particle particle, float elapsedSeconds)
        {
            float growthRate = 0.1f; // Adjust as needed
            particle.Scale += growthRate * elapsedSeconds;
        }

        public void DampVelocity(Particle particle, float elapsedSeconds)
        {
            float dampingFactor = 0.98f; // Adjust as needed
            particle.Velocity *= MathF.Pow(dampingFactor, elapsedSeconds);
        }

        public void SinusoidalMotion(Particle particle, float elapsedSeconds)
        {
            float frequency = 2.0f; // Adjust as needed
            float amplitude = 0.1f; // Adjust as needed
            particle.Position.Y += amplitude * MathF.Sin(frequency * particle.Position.X);
        }
        public void BounceOffWalls(Particle particle, float elapsedSeconds)
        {
            Vector2 screenSize = new Vector2(Window.ClientBounds.Width, Window.ClientBounds.Height); // Adjust as needed
            if (particle.Position.X < 0 || particle.Position.X > screenSize.X)
            {
                particle.Velocity.X *= -0.1f;
            }
            if (particle.Position.Y < 0 || particle.Position.Y > screenSize.Y - floor_height + RandomHelper.NextFloat(0,4))
            {
                particle.Velocity.Y = 0;
                particle.TotalFrames = 1;
                particle.Scale = 0.7f;
            }
        }

        public void RotateAroundPoint(Particle particle, float elapsedSeconds)
        {
            Vector2 rotationCenter = _ScreenCenter; // Adjust as needed
            float rotationSpeed = MathHelper.ToRadians(90); // Adjust as needed
            Vector2 offset = particle.Position - rotationCenter;
            Matrix rotationMatrix = Matrix.CreateRotationZ(rotationSpeed * elapsedSeconds);
            offset = Vector2.Transform(offset, rotationMatrix);
            particle.Position = rotationCenter + offset;
        }
        private void SetSnowAttributes(Particle particle)
        {
            particle.Position = new Vector2(RandomHelper.NextFloat(0, Window.ClientBounds.Width), 0);
            particle.Scale = (float)MathHelper.Clamp(RandomHelper.NextFloat(1f, 1.2f), 1f, 1.2f);
            particle.Lifespan = RandomHelper.NextFloat(5f, 100f);
            particle.Color = Color.White * RandomHelper.NextFloat(0.5f, 1f);
            particle.Velocity = new Vector2(0, RandomHelper.NextFloat(30f, 100f)); // Falling vertically
        }

        private void Friction(Particle particle,float elapsedSeconds)
        {
            particle.Velocity *= RandomHelper.NextFloat(0.96f,0.98f);
        }
        private void AttractToMouse(Particle particle, float elapsedSeconds)
        {
            // Get the current mouse state
            

            // Calculate the vector from the particle to the mouse position
            Vector2 repulsionForce = new Vector2(mouseState.X - particle.Position.X, mouseState.Y - particle.Position.Y);

            // Calculate the distance from the particle to the mouse
            float distanceToMouse = repulsionForce.Length();

            // Define a repulsion radius (you can experiment with different values)
            float repulsionRadius = 100f;
            
            // Check if the particle is within the repulsion radius
            if (distanceToMouse < repulsionRadius)
            {
                // Normalize the repulsion force and adjust the strength
                repulsionForce.Normalize();
                float repulsionStrength =0; // Experiment with different values
                if(lmousebutton)
                {
                    repulsionStrength = 800;
                } else if (rmousebutton)
                {
                    repulsionStrength = -1200;
                }
                if(!rmousebutton && heldsnowball != null) {
                    heldsnowball.RemoveAttributeModifier(HoldParticle);
                    heldsnowball = null;
                }
                // Update the particle's velocity based on the repulsion force
                particle.Velocity -= repulsionForce * repulsionStrength * elapsedSeconds;
            }
        }

        private void InfiniteLifespan(Particle particle,float t)
        {
            particle.Lifespan = 1;
        }

        public void SnowballModifier(Particle particle, float elapsedSeconds)
        {
            // Define gravity (you can adjust the value as needed)
            Vector2 gravity = new Vector2(0, 100f);

            // Update particle velocity based on gravity
            particle.Velocity += gravity * elapsedSeconds;

            // Update particle position based on velocity
            particle.Position += particle.Velocity * elapsedSeconds;

            // Add damping to simulate air resistance (you can adjust the value as needed)
            float dampingFactor = 0.98f;
            particle.Velocity *= dampingFactor;

            // Check for collisions with the window boundaries (you can customize this based on your game's world)
            // For simplicity, we'll assume the window has dimensions defined by Viewport.Width and Viewport.Height
            if (particle.Position.X < 0)
            {
                particle.Position.X = 0;
                particle.Velocity.X *= -1; // Reflect the velocity on collision
            }
            else if (particle.Position.X > Window.ClientBounds.Width)
            {
                particle.Position.X = Window.ClientBounds.Width;
                particle.Velocity.X *= -1; // Reflect the velocity on collision
            }

            if (particle.Position.Y < 0)
            {
                particle.Position.Y = 0;
                particle.Velocity.Y *= -1; // Reflect the velocity on collision
            }
            else if (particle.Position.Y > Window.ClientBounds.Height - floor_height)
            {
                for (int i = 0; i < SnowballDensity; i++)
                {
                    Snow.AddParticle(particle.Position + new Vector2(RandomHelper.NextFloat(-2,2), RandomHelper.NextFloat(-2, 2)), Vector2.Zero);
                }
                
                particle.Lifespan = 0;
            }
        }
        public void HoldParticle(Particle particle,float elapsedSeconds) {
            particle.Position = new Vector2(mouseState.X, mouseState.Y);
            particle.Velocity = mouseVelocity;
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