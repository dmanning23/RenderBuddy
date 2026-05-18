RenderBuddy
===========

A MonoGame library for drawing 2D sprites with normal-mapped dynamic lighting. Supports ambient light, directional lights, and point lights — including animated variants like fire flicker, flash, and full ADSR-style flares.

## Installation

Install the NuGet package:

```
https://www.nuget.org/packages/RenderBuddy/
```

You must also add `Content/AnimationBuddyShader.fx` to your MonoGame content project. The renderer loads it by name (`AnimationBuddyShader`) at runtime.

## Dependencies

RenderBuddy pulls in several companion libraries automatically via NuGet:

- `CameraBuddy` — camera and world transform
- `FilenameBuddy` — cross-platform filename handling
- `GameTimer` — countdown timers used by animated lights
- `MatrixExtensions` — matrix/vector helpers
- `PrimitiveBuddy` — debug primitive drawing
- `RandomExtensions.dmanning23` — used by fire/flare lights
- `ResolutionBuddy` — resolution-independent rendering
- `Vector2Extensions`

## Setup

```csharp
// Create and store the renderer (pass your Game's ContentManager)
IRenderer renderer = new Renderer(game, Content);

// Call in LoadContent
renderer.LoadContent(GraphicsDevice);

// Call in Update
renderer.Update(gameTime);

// Call in UnloadContent / on dispose
renderer.UnloadContent();
```

## Loading Textures

`TextureInfo` bundles a diffuse texture with optional normal map and color mask:

```csharp
// Diffuse only
TextureInfo image = renderer.LoadImage(new Filename("textures/hero"));

// With normal map
TextureInfo image = renderer.LoadImage(
    new Filename("textures/hero"),
    new Filename("textures/hero_n"));

// With normal map and color mask (for palette swapping)
TextureInfo image = renderer.LoadImage(
    new Filename("textures/hero"),
    new Filename("textures/hero_n"),
    new Filename("textures/hero_mask"));
```

## Drawing

Wrap draws in a `SpriteBatchBegin` / `SpriteBatchEnd` pair. The normal-mapped shader is applied automatically.

```csharp
renderer.SpriteBatchBegin(BlendState.AlphaBlend, camera.TranslationMatrix);

renderer.Draw(
    image,
    position,        // Vector2
    primaryColor,    // Color — tint applied to the diffuse texture
    secondaryColor,  // Color — multiplied into the color mask (palette swap)
    rotation,        // float, radians
    isFlipped,       // bool — flips the sprite and corrects normal map X axis
    scale,           // float
    layer);          // float — SpriteBatch sort layer

renderer.SpriteBatchEnd();
```

To draw without the lighting shader (e.g. UI elements):

```csharp
renderer.SpriteBatchBeginNoEffect(BlendState.AlphaBlend, Matrix.Identity);
```

## Lighting

### Ambient Light

Sets the minimum light level applied to all normal-mapped sprites.

```csharp
renderer.AmbientColor = new Color(0.35f, 0.35f, 0.35f);
```

### Directional Lights

Up to 3 directional lights are supported. A default downward warm-white light is added automatically.

```csharp
// direction is a Vector3 (normalized automatically); z controls depth of the light angle
renderer.AddDirectionalLight(new Vector3(0f, -1f, 0.2f), Color.White);
```

### Point Lights

Supports up to 4 point lights on desktop (32 on Android/iOS). Position Z controls the height of the light above the sprite plane — higher values spread light more broadly.

```csharp
PointLight light = renderer.AddPointLight(
    new Vector3(x, y, 100f),  // world position + height
    brightness,                // float — controls falloff radius
    Color.OrangeRed);
```

### Clearing Lights

```csharp
renderer.ClearLights();
```

## Animated Point Lights

### FirePointLight

Randomizes brightness on a timer, simulating fire flicker.

```csharp
var fire = new FirePointLight(
    position,      // Vector3
    Color.Orange,
    flareTimeDelta: 0.1f,   // seconds between brightness changes
    min: 50f,
    max: 150f);

renderer.AddPointLight(fire);
```

### FlashPointLight

Starts at full brightness and fades out to zero over a set duration, then marks itself dead.

```csharp
var flash = new FlashPointLight(
    position,
    brightness: 200f,
    Color.White,
    timeDelta: 0.3f);   // fade duration in seconds

renderer.AddPointLight(flash);
```

### FlarePointLight

Full ADSR-style light with attack, sustain, and delay phases, plus fire-flicker randomization during sustain. Supports an optional position delegate for attaching the light to a moving object.

```csharp
var flare = new FlarePointLight(
    position,
    Color.Yellow,
    flareTimeDelta: 0.05f,
    attackTimeDelta: 0.1f,
    sustainTimeDelta: 0.5f,   // pass -1 for infinite sustain
    delayTimeDelta: 0.2f,
    minFlare: 80f,
    maxFlare: 160f,
    positionDelegate: () => mySprite.Position3);  // optional

renderer.AddPointLight(flare);

// Manually end an infinite-sustain flare
flare.Kill();
```

Lights with a finite lifetime (`FlashPointLight`, `FlarePointLight`) remove themselves automatically from the renderer when dead.

## Shader

The included `AnimationBuddyShader.fx` is a pixel-only shader compatible with MonoGame's `SpriteBatch`. It supports:

- Ambient color
- Up to 5 directional lights with per-light color and rotation-corrected normals
- Up to 4 point lights on DirectX / 32 on OpenGL, with inverse-square falloff and rotation-corrected normals
- Optional color mask for palette swapping (blends a secondary color into masked regions)
- Horizontal flip correction (inverts the normal map X channel when the sprite is flipped)

## License

MIT
