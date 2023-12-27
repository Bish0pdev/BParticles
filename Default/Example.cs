using BParticleTemplate.Default.Packages;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;



namespace BParticles
{
    public class CheatMenu
    {
        private bool isOpen = false;

        public void Toggle()
        {
            isOpen = !isOpen;
        }

        public void Draw(Example game,SpriteFont font,SpriteBatch batch,GameTime gameTime,GraphicsDevice graphicsDevice)
        {
            if (isOpen)
            {   
                // Set the position for the debug information
                Vector2 debugPosition = new Vector2(10, 10);

                // Set the color for the debug information
                Color debugColor = Color.White;
                batch.Begin();

                batch.DrawString(font, $"FPS: {1f / gameTime.ElapsedGameTime.TotalSeconds:0}", debugPosition, debugColor);
                debugPosition.Y += font.LineSpacing;

                batch.DrawString(font, $"Draw Calls: {graphicsDevice.Metrics.DrawCount}", debugPosition, debugColor);
                debugPosition.Y += font.LineSpacing;
                // Draw each public variable's debug information
                batch.DrawString(font, $"SnowballRadius: {game.SnowballRadius}", debugPosition, debugColor);
                debugPosition.Y += font.LineSpacing;

                batch.DrawString(font, $"SnowballDensity: {game.SnowballDensity}", debugPosition, debugColor);
                debugPosition.Y += font.LineSpacing;

                batch.DrawString(font, $"veltobreak: {game.veltobreak}", debugPosition, debugColor);
                debugPosition.Y += font.LineSpacing;

                batch.DrawString(font, $"growamount: {game.growamount}", debugPosition, debugColor);
                debugPosition.Y += font.LineSpacing;

                batch.DrawString(font, $"Mouse X: {game.mouseState.X}, Mouse Y: {game.mouseState.Y}", debugPosition, debugColor);
                batch.End();
            }
        }
    }
    public class Example : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont font;
        private Texture2D pixelTexture;

        public ParticleSystem Snow;
        public ParticleSystem activeSnowballs;

        private Random random = new Random();
        private float elapsedSpawnTime = 0.0f;

        public MouseState mouseState;
        private bool rmousebutton, lmousebutton;
        private Vector2 mouseVelocity = Vector2.Zero;
        private Vector2 previousMousePosition = Vector2.Zero;

        public int SnowballRadius = 2;
        public int SnowballDensity = 5;
        public float veltobreak = 150f;
        public float growamount = 0.001f;


        private Vector2 _ScreenCenter;
        private Vector2 initialdir = Vector2.Zero;
        public int floor_height = 100;
        private List<Particle> particlesinradius = new List<Particle>();

        private CheatMenu debugmenu = new CheatMenu();
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
            Window.Title = "Snow!";
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
            activeSnowballs = new ParticleSystem(CreateCircleTexture(GraphicsDevice,SnowballRadius*100));
            activeSnowballs.AddSpawnModifier(x =>
            {
                x.Scale = 0.01f;
                x.Position = new Vector2(mouseState.X, mouseState.Y);
            });
            activeSnowballs.AddAttributeModifier(SnowballModifier);
            activeSnowballs.AddAttributeModifier(InfiniteLifespan);
            activeSnowballs.AddAttributeModifier(AttractToMouse);
            activeSnowballs.AddAttributeModifier(CollideWithOthers);
            activeSnowballs.SystemPosition = _ScreenCenter;

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

            mouseVelocity = (currentMousePosition - previousMousePosition) / (float)gameTime.ElapsedGameTime.TotalSeconds;

            previousMousePosition = currentMousePosition;

            if (Keyboard.GetState().IsKeyDown(Keys.F1))
            {
                // Destroy all active particles
                Snow.ClearParticles();
                activeSnowballs.ClearParticles();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.F2))
            {
                debugmenu.Toggle();
            }

            Snow.Update(gameTime);
            particlesinradius = Snow.ParticlesInRadius(new Vector2(mouseState.X, mouseState.Y), SnowballRadius);
            
            if (particlesinradius.Count >= SnowballDensity)
            {
                float particleSize = SnowballRadius * (particlesinradius.Count / SnowballDensity);
                // Create a single snowball system
                Particle newsnoball = activeSnowballs.AddParticle(activeSnowballs.spawnModifiers);
                newsnoball.Scale = SnowballDensity / (particleSize * Getactualsize(newsnoball));
                newsnoball.Scale *= .01f;
                // Remove particles in the radius from the Snow system
                foreach (var particle in particlesinradius)
                {
                    particle.RemoveParticle();
                }
            }

            activeSnowballs.Update(gameTime);

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
            activeSnowballs.Draw(_spriteBatch);
            _spriteBatch.End();

            debugmenu.Draw(this,font,_spriteBatch,gameTime,GraphicsDevice);

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

        private float Getactualsize(Particle particle)
        {
            return SnowballRadius * (particle.Scale * 100);
        }

        private const float Gravity = 100f;
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
        private void SetSnowAttributes(Particle particle)
        {
            if (particle.Position == particle.parentSystem.SystemPosition)
            {
                particle.Position = new Vector2(RandomHelper.NextFloat(0, Window.ClientBounds.Width), 0);
            }
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
                // Update the particle's velocity based on the repulsion force
                particle.Velocity -= repulsionForce * repulsionStrength * elapsedSeconds;
            }
        }
        public void CollideWithOthers(Particle particle, float elapsedSeconds)
        {
            // Check for collisions with other particles in the system
            if (particle.parentSystem != null)
            {
                for (int i = 0; i < particle.parentSystem.particles.Count; i++)
                {
                    Particle otherParticle = particle.parentSystem.particles[i];
                    if (particle != otherParticle)
                    {
                        float distance = Vector2.Distance(particle.Position, otherParticle.Position);
                        float combinedRadius = Getactualsize(particle) + SnowballRadius * (otherParticle.Scale * 100);

                        if (distance < combinedRadius)
                        {
                            // Calculate the collision normal
                            Vector2 collisionNormal = Vector2.Normalize(particle.Position - otherParticle.Position);

                            // Calculate the overlap distance
                            float overlap = combinedRadius - distance;

                            // Move particles along the collision normal to separate them
                            particle.Position += collisionNormal * (overlap / 2);
                            otherParticle.Position -= collisionNormal * (overlap / 2);
                        }
                    }
                }
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

            

            // Add damping to simulate air resistance (you can adjust the value as needed)
            float dampingFactor = 0.985f;
            particle.Velocity *= dampingFactor;
            // Check for particles in radius
            float actualscale = Getactualsize(particle);
            if (particle.Position.X <= 0 + actualscale || particle.Position.X >= Window.ClientBounds.Width - actualscale ||
                particle.Position.Y >= Window.ClientBounds.Height - floor_height - actualscale)
            {
                if (particle.Velocity.Y > veltobreak)
                {
                    // Create burst particles in a circle
                    BurstSnowball(particle, actualscale);
                    
                }
                else
                {
                    // Keep the particle  from going into the floor
                    particle.Position.Y = Math.Min(particle.Position.Y, Window.ClientBounds.Height - floor_height - actualscale);
                    if (particle.Velocity.Y < 0)
                    {
                        particle.Velocity.Y = 0f;
                    }
                }
            }
            particle.Position += particle.Velocity * elapsedSeconds;

            List<Particle> list = Snow.ParticlesInRadius(particle.Position, actualscale);
            if (list.Count != 0)
            {
                foreach (Particle item in list)
                {
                    Snow.particles.Remove(item);

                    particle.Scale += growamount;
                    particle.Position.Y += growamount;

                }

            }
        }

        private void BurstSnowball(Particle particle, float scale)
        {
            
            for (int i = 0; i < (int)Math.Ceiling(scale*100); i++)
            {
                float angle = MathHelper.TwoPi * i / (scale * 100);
                float radius = RandomHelper.NextFloat(0, scale);

                float offsetX = (float)Math.Cos(angle) * radius;
                float offsetY = (float)Math.Sin(angle) * radius;

                Vector2 offset = new Vector2(offsetX, offsetY);

                Snow.AddParticle(particle.Position + offset, Vector2.Zero).Velocity = particle.Velocity;
            }
            particle.RemoveParticle();
        }

        public void HoldParticle(Particle particle,float elapsedSeconds) {
            particle.Position = new Vector2(mouseState.X, mouseState.Y);
            particle.Velocity = mouseVelocity/5;
        }
        #endregion
        public static Texture2D CreateCircleTexture(GraphicsDevice graphicsDevice, int circleRadius)
        {
            int diameter = circleRadius * 2;

            // Create a new texture for the circle
            Texture2D circleTexture = new Texture2D(graphicsDevice, diameter, diameter);

            Color[] circlePixels = new Color[diameter * diameter];

            for (int x = 0; x < diameter; x++)
            {
                for (int y = 0; y < diameter; y++)
                {
                    // Calculate the distance from the center of the circle
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(circleRadius, circleRadius));

                    // Map the 2D coordinates to the 1D array index
                    int index = x + y * diameter;

                    // Check if the pixel is inside the circle
                    if (distance <= circleRadius)
                    {
                        // Set the pixel color for the circular texture
                        circlePixels[index] = Color.White; // You can set any color here
                    }
                    else
                    {
                        // Set pixels outside the circle to transparent or any desired color
                        circlePixels[index] = Color.Transparent;
                    }
                }
            }

            // Set the data for the circular texture
            circleTexture.SetData(circlePixels);

            return circleTexture;
        }
    }

}