# PainterlyBuddy — Design Spec

**Date:** 2026-05-18  
**Status:** Draft — awaiting user review  
**Target library:** `PainterlyBuddy` (new MonoGame NuGet package, separate from RenderBuddy)

---

## Overview

PainterlyBuddy is a MonoGame post-processing library that applies a real-time oil painting effect to the entire rendered scene. It uses a Kuwahara filter — a neighbourhood filter that finds the region of least color variance around each pixel and outputs its mean, producing the characteristic "painted regions with organic edges" appearance of oil painting.

The effect is a full-screen post-process: it captures the game's render output to a render target, then applies the shader before the result reaches the screen. This means it affects backgrounds, characters, particles, and lighting together, which is what gives it its cohesive painted look.

PainterlyBuddy has no dependency on RenderBuddy. It depends only on MonoGame.

---

## Architectural Fit

This library belongs alongside other full-screen post-processing effects (bloom, scanlines, vignette, etc.) rather than in RenderBuddy, which is scoped to per-character lighting with normal maps.

```
Game
 └── RenderBuddy (per-character lighting, normal maps)
 └── PainterlyBuddy (full-screen post-process)   ← this library
 └── [future: BloomBuddy, etc.]
```

---

## Components

### 1. `PainterlySettings` (struct)

Holds all shader parameters. Lightweight value type — safe to copy, lerp, and store.

```csharp
public struct PainterlySettings
{
    // Radius of each Kuwahara quadrant in pixels. Range: 1–8.
    // Higher = larger paint regions = more abstracted/painterly.
    public int KernelRadius;

    // Number of filter passes. 1 or 2.
    // Two passes dramatically increases the painted effect with modest extra cost.
    public int Passes;

    // Saturation multiplier applied after filtering. Range: 1.0–2.0.
    // Oil paintings have rich, vivid color — boost this to sell the look.
    public float SaturationBoost;

    // Opacity of the procedural canvas/linen weave texture overlay. Range: 0.0–1.0.
    public float CanvasStrength;

    // When true, uses anisotropic Kuwahara: sample sectors rotate to follow
    // local edge direction, so brush strokes flow along shapes rather than
    // being axis-aligned. More expensive; required for the Expressive preset.
    public bool Anisotropic;
}
```

#### Static Presets

| Parameter | Subtle | Classic | Expressive |
|---|---|---|---|
| KernelRadius | 2 | 4 | 6 |
| Passes | 1 | 1 | 2 |
| SaturationBoost | 1.1 | 1.4 | 1.8 |
| CanvasStrength | 0.15 | 0.35 | 0.55 |
| Anisotropic | false | false | true |

#### Lerp

```csharp
public static PainterlySettings Lerp(PainterlySettings a, PainterlySettings b, float t)
```

- `KernelRadius`: integer lerp (round to nearest)
- `Passes`: use `a.Passes` when `t < 0.5`, `b.Passes` when `t >= 0.5`
- `SaturationBoost`, `CanvasStrength`: `MathHelper.Lerp`
- `Anisotropic`: use `a.Anisotropic` when `t < 0.5`, `b.Anisotropic` when `t >= 0.5`

---

### 2. `PainterlyEffect` (class)

Owns the render target, shader, and ping-pong buffer. Manages the capture → filter → output loop.

```csharp
public class PainterlyEffect : IDisposable
{
    public PainterlySettings Settings { get; set; }  // swap or lerp at any time

    public PainterlyEffect(GraphicsDevice graphics, ContentManager content);

    // Redirect all subsequent rendering into the capture render target.
    // Call this before your SpriteBatchBegin / draw calls.
    public void BeginCapture();

    // Restore the backbuffer as the render target, apply the painterly shader
    // (one or two passes per Settings.Passes), and draw the result to screen.
    // Call this after SpriteBatchEnd.
    public void Apply();

    public void Dispose();
}
```

#### Internal render target lifecycle

- One `RenderTarget2D _capture` — receives the game's draw output.
- One `RenderTarget2D _pingPong` — used as intermediate when `Settings.Passes == 2`.
- Both sized to `Viewport.Width × Viewport.Height` at construction.
- Recreated if the viewport size changes (check in `Apply()`).

#### Multi-pass flow

```
Passes == 1:   _capture ──[shader]──► screen
Passes == 2:   _capture ──[shader]──► _pingPong ──[shader]──► screen
```

Both passes use the same shader and the same `Settings`. The second pass processes already-filtered pixels, compounding the effect.

---

### 3. `PainterlyShader.fx` (HLSL)

A pixel-only shader applied as a full-screen SpriteBatch draw. Same OpenGL/DirectX compatibility block as RenderBuddy's existing shader.

```hlsl
#if OPENGL
  #define PS_SHADERMODEL ps_3_0
#else
  #define PS_SHADERMODEL ps_4_0
#endif
```

#### Parameters (set from C# via `Effect.Parameters`)

```hlsl
float2 TexelSize;        // (1.0/viewportWidth, 1.0/viewportHeight)
int    KernelRadius;     // 1–8
float  SaturationBoost;  // 1.0–2.0
float  CanvasStrength;   // 0.0–1.0
bool   Anisotropic;      // edge-aware sector rotation
```

#### Kuwahara Filter — Standard (Anisotropic == false)

For each output pixel at UV `uv`:

1. Define 4 overlapping square quadrants of size `(KernelRadius+1) × (KernelRadius+1)`:
   - Q0: offsets x ∈ [−r, 0], y ∈ [−r, 0]
   - Q1: offsets x ∈ [0, r],  y ∈ [−r, 0]
   - Q2: offsets x ∈ [−r, 0], y ∈ [0, r]
   - Q3: offsets x ∈ [0, r],  y ∈ [0, r]

2. For each quadrant, sample all `(r+1)²` pixels and compute:
   - `mean`     = average RGB
   - `variance` = average of `(sample.rgb − mean)²`, reduced to a scalar via `dot(variance, float3(1,1,1)/3.0)`

3. Output the `mean` of the quadrant with the lowest `variance`.

**Loop unrolling:** HLSL requires compile-time loop bounds. Use a fixed max radius (8) with a runtime early-exit (`if (i > KernelRadius) break`), or define separate technique passes per radius. The fixed-max approach is simpler and acceptable for this use case.

**ps_3_0 instruction budget:** With `KernelRadius = 6`, each quadrant samples 49 pixels — 196 texture fetches total. This is at the edge of ps_3_0's instruction limit. If targeting OpenGL/DesktopGL, cap `KernelRadius` at 4 (81 fetches/quadrant, 324 total) in the C# layer when on OpenGL and `Anisotropic == false`. Document this limit clearly.

#### Kuwahara Filter — Anisotropic (Anisotropic == true)

Anisotropic Kuwahara rotates each quadrant's sample region to align with the local edge direction, so brush strokes follow the shapes in the image.

1. **Estimate edge direction** using 4-sample finite differences on luminance:
   ```hlsl
   float lum(float4 c) { return dot(c.rgb, float3(0.299, 0.587, 0.114)); }
   float gx = lum(tex2D(s, uv + float2( dx, 0))) - lum(tex2D(s, uv + float2(-dx, 0)));
   float gy = lum(tex2D(s, uv + float2(0,  dy))) - lum(tex2D(s, uv + float2(0, -dy)));
   float angle = atan2(gy, gx);
   ```
   where `dx = TexelSize.x`, `dy = TexelSize.y`.

2. **Build a 2D rotation matrix** from `angle`:
   ```hlsl
   float cs = cos(angle), sn = sin(angle);
   float2x2 R = float2x2(cs, -sn, sn, cs);
   ```

3. **Apply rotation to each quadrant's pixel offsets** before multiplying by `TexelSize`:
   ```hlsl
   float2 offset = mul(R, float2(i, j)) * TexelSize;
   float4 sample = tex2D(Sampler, uv + offset);
   ```

4. Proceed with the same mean/variance/selection logic as the standard filter.

**Note:** Anisotropic mode requires `ps_4_0` (DirectX only) due to `atan2` and the additional texture fetches. It must not be enabled on OpenGL targets. `PainterlyEffect.Apply()` should check the graphics profile and ignore `Settings.Anisotropic = true` on OpenGL, applying standard Kuwahara instead.

#### Saturation Boost

Applied after Kuwahara, before canvas overlay:

```hlsl
float gray = dot(color.rgb, float3(0.299, 0.587, 0.114));
color.rgb = lerp(float3(gray, gray, gray), color.rgb, SaturationBoost);
```

#### Procedural Canvas Texture

Simulates linen/canvas weave without requiring a texture asset:

```hlsl
float hStrands = sin(texCoord.y * 800.0 * 3.14159);
float vStrands = sin(texCoord.x * 800.0 * 3.14159);
float canvas   = (hStrands * vStrands) * 0.5 + 0.5;  // remap to [0, 1]
color.rgb *= lerp(1.0, canvas, CanvasStrength);
```

The frequency constant `800.0` targets a canvas-weave period of roughly 1–2 pixels at 1080p — adjust if needed for different resolutions. This can be exposed as a parameter in a future revision.

#### Technique

```hlsl
technique Painterly
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
```

---

## Usage Example

```csharp
// Setup
var painterly = new PainterlyEffect(GraphicsDevice, Content);
painterly.Settings = PainterlySettings.Classic;

// Runtime transition (e.g. in Update)
float t = MathHelper.Clamp(transitionTimer / transitionDuration, 0f, 1f);
painterly.Settings = PainterlySettings.Lerp(PainterlySettings.Classic, PainterlySettings.Expressive, t);

// Draw loop
painterly.BeginCapture();

    renderer.SpriteBatchBegin(BlendState.AlphaBlend, camera.TranslationMatrix);
    // ... draw characters, world, particles ...
    renderer.SpriteBatchEnd();

painterly.Apply();   // painterly effect hits everything above

// Draw HUD after Apply() if you want UI unaffected
spriteBatch.Begin();
// ... draw HUD ...
spriteBatch.End();
```

---

## Content Pipeline

`PainterlyShader.fx` must be compiled via the MonoGame Content Pipeline (MGCB) and loaded via `ContentManager`, the same as `AnimationBuddyShader.fx` in RenderBuddy.

Add to the game's `.mgcb` file:

```
/build:Shaders/PainterlyShader.fx
```

---

## Dependencies

- `MonoGame.Framework.DesktopGL` (or platform equivalent) — only dependency
- No dependency on RenderBuddy, CameraBuddy, or any other Buddy library

---

## Out of Scope

- Bloom, vignette, scanlines — separate libraries
- Per-sprite painterly (not a post-process) — architecturally different, much harder, not planned
- Specular highlights via Kuwahara — not meaningful in a 2D top-down context
- Runtime viewport resize handling beyond v1 — documented limitation, recreate `PainterlyEffect` on resize
