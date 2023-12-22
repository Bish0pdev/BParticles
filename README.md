# BParticles

## *Small Customizable Particle System For Monogame*

BParticles is a simple and expandable framework for using particles in Monogame.

**THIS PROJECT IS STILL VERY EARLY AND SHOULD NOT BE USED IN ANY COMMERCIAL PROJECTS**

## Usage

Simply copy the [Packages](Package) folder into your own monogame project. You can also clone the "HelloWorldProject" branch and run it yourself in visual studio.

Once set up in your own project, creating a new particle system in BParticles can be done in as little as *4* lines of code.

1) Instantiate the new system (particleTexture can be any Texture2D)
```
_particleSystem = new ParticleSystem(particleTexture);
```

2) Update the system in the Update() method in your game
```
_particleSystem.Update(gameTime);
```

3) Render the system in the Draw() function
```
_spriteBatch.Begin();
_particleSystem.Draw(_spriteBatch);
_spriteBatch.End();
```

4) Play it from anywhere in your code!
```
_particleSystem.Play();
```

**Here is the code to set up the example particle system given in the [ExampleScene](ExampleScene.cs)

```
_particleSystem = new ParticleSystem(particleTexture);
_particleSystem.AddSpawnModifier(RandomColor);
_particleSystem.AddSpawnModifier(x => x.Velocity = GetRandomVector(-50f,50));
_particleSystem.AddSpawnModifier(x => x.Lifespan = 1f);
_particleSystem.SystemPosition = _ScreenCenter;
_particleSystem.Play();
```

*The package should be fully docstringed but full documentation is planned, feel free to use the issue board for any feature requests or if you find any issues*