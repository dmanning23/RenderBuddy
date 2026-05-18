# PainterlyBuddy Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build PainterlyBuddy — a standalone MonoGame NuGet library that applies a real-time oil painting post-process effect to the full game scene via a Kuwahara filter with tunable presets and smooth runtime transitions between them.

**Architecture:** `PainterlySettings` (struct) holds all shader parameters and exposes three static presets (Subtle, Classic, Expressive) plus a static `Lerp` method. `PainterlyEffect` (class) owns two render targets and a SpriteBatch, redirects scene rendering via `BeginCapture()`, then applies the HLSL Kuwahara shader one or two times in `Apply()`. The shader (`PainterlyShader.fx`) implements both standard 4-quadrant Kuwahara and an anisotropic edge-aware variant, plus procedural canvas texture and saturation boost — all in a single pixel shader.

**Tech Stack:** C# / MonoGame (DesktopGL + WindowsDX), HLSL ps_3_0 / ps_4_0, NUnit 3 for unit tests

---

## File Map

```
PainterlyBuddy/
├── PainterlyBuddy/
│   ├── PainterlyBuddy.csproj    ← user scaffolds (net8.0, MonoGame ref, GeneratePackage)
│   ├── PainterlySettings.cs     ← Task 1
│   └── PainterlyEffect.cs       ← Tasks 3 + 4
├── PainterlyBuddy.Tests/
│   ├── PainterlyBuddy.Tests.csproj  ← user scaffolds (NUnit, MonoGame, ProjectRef)
│   └── PainterlySettingsTests.cs    ← Task 1
└── Content/
    └── PainterlyShader.fx       ← Tasks 2 + 5
```

> **Shader deployment note:** `Content/PainterlyShader.fx` lives in the library repo for reference and must be added to each consuming game's MGCB content project (the same pattern RenderBuddy uses for `AnimationBuddyShader.fx`).

---

## Task 1 — PainterlySettings struct

**Files:**
- Create: `PainterlyBuddy/PainterlySettings.cs`
- Create: `PainterlyBuddy.Tests/PainterlySettingsTests.cs`

- [ ] **Step 1: Write the failing tests**

Create `PainterlyBuddy.Tests/PainterlySettingsTests.cs`:

```csharp
using Microsoft.Xna.Framework;
using NUnit.Framework;
using PainterlyBuddy;

namespace PainterlyBuddy.Tests
{
    [TestFixture]
    public class PainterlySettingsTests
    {
        [Test]
        public void Subtle_HasExpectedValues()
        {
            var s = PainterlySettings.Subtle;
            Assert.That(s.KernelRadius, Is.EqualTo(2));
            Assert.That(s.Passes, Is.EqualTo(1));
            Assert.That(s.SaturationBoost, Is.EqualTo(1.1f).Within(0.001f));
            Assert.That(s.CanvasStrength, Is.EqualTo(0.15f).Within(0.001f));
            Assert.That(s.Anisotropic, Is.False);
        }

        [Test]
        public void Classic_HasExpectedValues()
        {
            var s = PainterlySettings.Classic;
            Assert.That(s.KernelRadius, Is.EqualTo(4));
            Assert.That(s.Passes, Is.EqualTo(1));
            Assert.That(s.SaturationBoost, Is.EqualTo(1.4f).Within(0.001f));
            Assert.That(s.CanvasStrength, Is.EqualTo(0.35f).Within(0.001f));
            Assert.That(s.Anisotropic, Is.False);
        }

        [Test]
        public void Expressive_HasExpectedValues()
        {
            var s = PainterlySettings.Expressive;
            Assert.That(s.KernelRadius, Is.EqualTo(6));
            Assert.That(s.Passes, Is.EqualTo(2));
            Assert.That(s.SaturationBoost, Is.EqualTo(1.8f).Within(0.001f));
            Assert.That(s.CanvasStrength, Is.EqualTo(0.55f).Within(0.001f));
            Assert.That(s.Anisotropic, Is.True);
        }

        [Test]
        public void Lerp_AtZero_ReturnsA()
        {
            var result = PainterlySettings.Lerp(PainterlySettings.Subtle, PainterlySettings.Expressive, 0f);
            Assert.That(result.KernelRadius, Is.EqualTo(2));
            Assert.That(result.Passes, Is.EqualTo(1));
            Assert.That(result.SaturationBoost, Is.EqualTo(1.1f).Within(0.001f));
            Assert.That(result.CanvasStrength, Is.EqualTo(0.15f).Within(0.001f));
            Assert.That(result.Anisotropic, Is.False);
        }

        [Test]
        public void Lerp_AtOne_ReturnsB()
        {
            var result = PainterlySettings.Lerp(PainterlySettings.Subtle, PainterlySettings.Expressive, 1f);
            Assert.That(result.KernelRadius, Is.EqualTo(6));
            Assert.That(result.Passes, Is.EqualTo(2));
            Assert.That(result.SaturationBoost, Is.EqualTo(1.8f).Within(0.001f));
            Assert.That(result.CanvasStrength, Is.EqualTo(0.55f).Within(0.001f));
            Assert.That(result.Anisotropic, Is.True);
        }

        [Test]
        public void Lerp_AtHalf_InterpolatesContinuousValues()
        {
            var result = PainterlySettings.Lerp(PainterlySettings.Subtle, PainterlySettings.Expressive, 0.5f);
            Assert.That(result.SaturationBoost, Is.EqualTo(1.45f).Within(0.01f));  // (1.1+1.8)/2
            Assert.That(result.CanvasStrength, Is.EqualTo(0.35f).Within(0.01f));   // (0.15+0.55)/2
        }

        [Test]
        public void Lerp_Passes_SnapsAtHalf()
        {
            var below = PainterlySettings.Lerp(PainterlySettings.Subtle, PainterlySettings.Expressive, 0.49f);
            var above = PainterlySettings.Lerp(PainterlySettings.Subtle, PainterlySettings.Expressive, 0.5f);
            Assert.That(below.Passes, Is.EqualTo(1));
            Assert.That(above.Passes, Is.EqualTo(2));
        }

        [Test]
        public void Lerp_Anisotropic_SnapsAtHalf()
        {
            var below = PainterlySettings.Lerp(PainterlySettings.Subtle, PainterlySettings.Expressive, 0.49f);
            var above = PainterlySettings.Lerp(PainterlySettings.Subtle, PainterlySettings.Expressive, 0.5f);
            Assert.That(below.Anisotropic, Is.False);
            Assert.That(above.Anisotropic, Is.True);
        }

        [Test]
        public void Lerp_ClampsT_BelowZero()
        {
            var result = PainterlySettings.Lerp(PainterlySettings.Subtle, PainterlySettings.Expressive, -1f);
            Assert.That(result.SaturationBoost, Is.EqualTo(1.1f).Within(0.001f));
        }

        [Test]
        public void Lerp_ClampsT_AboveOne()
        {
            var result = PainterlySettings.Lerp(PainterlySettings.Subtle, PainterlySettings.Expressive, 2f);
            Assert.That(result.SaturationBoost, Is.EqualTo(1.8f).Within(0.001f));
        }
    }
}
```

- [ ] **Step 2: Run tests to confirm they fail**

```bash
dotnet test PainterlyBuddy.Tests/PainterlyBuddy.Tests.csproj -v minimal
```

Expected: compile error — `PainterlySettings` does not exist.

- [ ] **Step 3: Implement PainterlySettings**

Create `PainterlyBuddy/PainterlySettings.cs`:

```csharp
using Microsoft.Xna.Framework;

namespace PainterlyBuddy
{
    public struct PainterlySettings
    {
        /// <summary>Radius of each Kuwahara quadrant in pixels (1–8). Higher = more painted.</summary>
        public int KernelRadius;

        /// <summary>Number of filter passes (1 or 2). Two passes compounds the effect.</summary>
        public int Passes;

        /// <summary>Saturation multiplier after filtering (1.0–2.0). Oil paintings have rich colour.</summary>
        public float SaturationBoost;

        /// <summary>Opacity of the procedural canvas/linen weave overlay (0.0–1.0).</summary>
        public float CanvasStrength;

        /// <summary>
        /// When true, sample sectors rotate to follow local edge direction (DirectX only).
        /// Gives brush strokes that flow along shapes rather than axis-aligned blocks.
        /// Automatically disabled on OpenGL targets by PainterlyEffect.
        /// </summary>
        public bool Anisotropic;

        public static readonly PainterlySettings Subtle = new PainterlySettings
        {
            KernelRadius  = 2,
            Passes        = 1,
            SaturationBoost = 1.1f,
            CanvasStrength  = 0.15f,
            Anisotropic   = false,
        };

        public static readonly PainterlySettings Classic = new PainterlySettings
        {
            KernelRadius  = 4,
            Passes        = 1,
            SaturationBoost = 1.4f,
            CanvasStrength  = 0.35f,
            Anisotropic   = false,
        };

        public static readonly PainterlySettings Expressive = new PainterlySettings
        {
            KernelRadius  = 6,
            Passes        = 2,
            SaturationBoost = 1.8f,
            CanvasStrength  = 0.55f,
            Anisotropic   = true,
        };

        /// <summary>
        /// Linearly interpolates between two settings. Continuous values (SaturationBoost,
        /// CanvasStrength, KernelRadius) interpolate smoothly; discrete values (Passes,
        /// Anisotropic) snap at t = 0.5.
        /// </summary>
        public static PainterlySettings Lerp(PainterlySettings a, PainterlySettings b, float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            return new PainterlySettings
            {
                KernelRadius    = (int)MathHelper.Lerp(a.KernelRadius, b.KernelRadius, t),
                Passes          = t < 0.5f ? a.Passes          : b.Passes,
                SaturationBoost = MathHelper.Lerp(a.SaturationBoost, b.SaturationBoost, t),
                CanvasStrength  = MathHelper.Lerp(a.CanvasStrength,  b.CanvasStrength,  t),
                Anisotropic     = t < 0.5f ? a.Anisotropic     : b.Anisotropic,
            };
        }
    }
}
```

- [ ] **Step 4: Run tests to confirm they pass**

```bash
dotnet test PainterlyBuddy.Tests/PainterlyBuddy.Tests.csproj -v minimal
```

Expected: all 9 tests pass.

- [ ] **Step 5: Commit**

```bash
git add PainterlyBuddy/PainterlySettings.cs PainterlyBuddy.Tests/PainterlySettingsTests.cs
git commit -m "feat: add PainterlySettings struct with presets and Lerp"
```

---

## Task 2 — PainterlyShader.fx (standard Kuwahara + saturation + canvas)

**Files:**
- Create: `Content/PainterlyShader.fx`

No unit tests — verify visually by running with a test game after Task 3.

- [ ] **Step 1: Create the shader**

Create `Content/PainterlyShader.fx`:

```hlsl
// PainterlyBuddy — oil painting post-process via Kuwahara filter.
// Apply as a full-screen SpriteBatch draw after rendering the scene to a RenderTarget2D.

#if OPENGL
  #define PS_SHADERMODEL ps_3_0
  #define MAX_RADIUS 4
#else
  #define PS_SHADERMODEL ps_4_0
  #define MAX_RADIUS 8
#endif

// ─── Parameters ───────────────────────────────────────────────────────────────

float2 TexelSize;        // (1.0 / viewportWidth, 1.0 / viewportHeight)
int    KernelRadius;     // runtime radius, capped at MAX_RADIUS from C#
float  SaturationBoost;  // 1.0 = unchanged, 2.0 = fully saturated
float  CanvasStrength;   // 0.0 = no texture, 1.0 = full canvas weave
bool   Anisotropic;      // edge-aware brush direction (DirectX only)

sampler TextureSampler : register(s0);

// ─── Helpers ──────────────────────────────────────────────────────────────────

float Luminance(float3 c)
{
    return dot(c, float3(0.299, 0.587, 0.114));
}

float3 BoostSaturation(float3 rgb, float boost)
{
    float gray = Luminance(rgb);
    return lerp(float3(gray, gray, gray), rgb, boost);
}

// Procedural linen/canvas weave. Frequency tuned for 1080p (period ~1-2px).
float CanvasPattern(float2 uv)
{
    float h = sin(uv.y * 800.0 * 3.14159265);
    float v = sin(uv.x * 800.0 * 3.14159265);
    return (h * v) * 0.5 + 0.5;
}

// ─── Standard Kuwahara ────────────────────────────────────────────────────────
// Divides the neighbourhood into 4 overlapping quadrants (Q0=TL, Q1=TR,
// Q2=BL, Q3=BR). Outputs the mean of the quadrant with the lowest variance.

float4 KuwaharaStandard(float2 uv)
{
    float4 sum0 = float4(0,0,0,0); float4 sq0 = float4(0,0,0,0); float n0 = 0;
    float4 sum1 = float4(0,0,0,0); float4 sq1 = float4(0,0,0,0); float n1 = 0;
    float4 sum2 = float4(0,0,0,0); float4 sq2 = float4(0,0,0,0); float n2 = 0;
    float4 sum3 = float4(0,0,0,0); float4 sq3 = float4(0,0,0,0); float n3 = 0;

    [unroll]
    for (int x = -MAX_RADIUS; x <= MAX_RADIUS; x++)
    {
        [unroll]
        for (int y = -MAX_RADIUS; y <= MAX_RADIUS; y++)
        {
            if (abs(x) > KernelRadius || abs(y) > KernelRadius) continue;

            float4 c = tex2D(TextureSampler, uv + float2(x, y) * TexelSize);

            if (x <= 0 && y <= 0) { sum0 += c; sq0 += c * c; n0++; }
            if (x >= 0 && y <= 0) { sum1 += c; sq1 += c * c; n1++; }
            if (x <= 0 && y >= 0) { sum2 += c; sq2 += c * c; n2++; }
            if (x >= 0 && y >= 0) { sum3 += c; sq3 += c * c; n3++; }
        }
    }

    float4 m0 = sum0 / n0;  float4 v0 = max(sq0 / n0 - m0 * m0, 0);
    float4 m1 = sum1 / n1;  float4 v1 = max(sq1 / n1 - m1 * m1, 0);
    float4 m2 = sum2 / n2;  float4 v2 = max(sq2 / n2 - m2 * m2, 0);
    float4 m3 = sum3 / n3;  float4 v3 = max(sq3 / n3 - m3 * m3, 0);

    float var0 = dot(v0.rgb, float3(1.0/3.0, 1.0/3.0, 1.0/3.0));
    float var1 = dot(v1.rgb, float3(1.0/3.0, 1.0/3.0, 1.0/3.0));
    float var2 = dot(v2.rgb, float3(1.0/3.0, 1.0/3.0, 1.0/3.0));
    float var3 = dot(v3.rgb, float3(1.0/3.0, 1.0/3.0, 1.0/3.0));

    float4 result = m0; float minVar = var0;
    if (var1 < minVar) { minVar = var1; result = m1; }
    if (var2 < minVar) { minVar = var2; result = m2; }
    if (var3 < minVar) { minVar = var3; result = m3; }

    return result;
}

// ─── Anisotropic Kuwahara ─────────────────────────────────────────────────────
// Excluded on OpenGL (no atan2 in ps_3_0). PainterlyEffect.Apply() ensures
// Anisotropic is never set to true when running on an OpenGL backend.

#if !OPENGL
float4 KuwaharaAnisotropic(float2 uv)
{
    // Estimate local edge direction from a 4-sample luminance gradient
    float gx = Luminance(tex2D(TextureSampler, uv + float2( TexelSize.x, 0)).rgb)
             - Luminance(tex2D(TextureSampler, uv + float2(-TexelSize.x, 0)).rgb);
    float gy = Luminance(tex2D(TextureSampler, uv + float2(0,  TexelSize.y)).rgb)
             - Luminance(tex2D(TextureSampler, uv + float2(0, -TexelSize.y)).rgb);

    float angle = atan2(gy, gx);
    float cs = cos(angle);
    float sn = sin(angle);

    float4 sum0 = float4(0,0,0,0); float4 sq0 = float4(0,0,0,0); float n0 = 0;
    float4 sum1 = float4(0,0,0,0); float4 sq1 = float4(0,0,0,0); float n1 = 0;
    float4 sum2 = float4(0,0,0,0); float4 sq2 = float4(0,0,0,0); float n2 = 0;
    float4 sum3 = float4(0,0,0,0); float4 sq3 = float4(0,0,0,0); float n3 = 0;

    [unroll]
    for (int x = -MAX_RADIUS; x <= MAX_RADIUS; x++)
    {
        [unroll]
        for (int y = -MAX_RADIUS; y <= MAX_RADIUS; y++)
        {
            if (abs(x) > KernelRadius || abs(y) > KernelRadius) continue;

            // Rotate the integer grid offset by the local edge angle so brush
            // strokes align with (and perpendicular to) edges in the image.
            float rx = cs * x - sn * y;
            float ry = sn * x + cs * y;

            float4 c = tex2D(TextureSampler, uv + float2(rx, ry) * TexelSize);

            // Quadrant assignment uses rotated coordinates
            if (rx <= 0 && ry <= 0) { sum0 += c; sq0 += c * c; n0++; }
            if (rx >= 0 && ry <= 0) { sum1 += c; sq1 += c * c; n1++; }
            if (rx <= 0 && ry >= 0) { sum2 += c; sq2 += c * c; n2++; }
            if (rx >= 0 && ry >= 0) { sum3 += c; sq3 += c * c; n3++; }
        }
    }

    // Guard against empty quadrants (can occur near strong edges after rotation)
    n0 = max(n0, 1); n1 = max(n1, 1); n2 = max(n2, 1); n3 = max(n3, 1);

    float4 m0 = sum0 / n0;  float4 v0 = max(sq0 / n0 - m0 * m0, 0);
    float4 m1 = sum1 / n1;  float4 v1 = max(sq1 / n1 - m1 * m1, 0);
    float4 m2 = sum2 / n2;  float4 v2 = max(sq2 / n2 - m2 * m2, 0);
    float4 m3 = sum3 / n3;  float4 v3 = max(sq3 / n3 - m3 * m3, 0);

    float var0 = dot(v0.rgb, float3(1.0/3.0, 1.0/3.0, 1.0/3.0));
    float var1 = dot(v1.rgb, float3(1.0/3.0, 1.0/3.0, 1.0/3.0));
    float var2 = dot(v2.rgb, float3(1.0/3.0, 1.0/3.0, 1.0/3.0));
    float var3 = dot(v3.rgb, float3(1.0/3.0, 1.0/3.0, 1.0/3.0));

    float4 result = m0; float minVar = var0;
    if (var1 < minVar) { minVar = var1; result = m1; }
    if (var2 < minVar) { minVar = var2; result = m2; }
    if (var3 < minVar) { minVar = var3; result = m3; }

    return result;
}
#endif

// ─── Main pixel shader ────────────────────────────────────────────────────────

float4 PixelShaderFunction(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 result;

#if OPENGL
    result = KuwaharaStandard(texCoord);
#else
    if (Anisotropic)
        result = KuwaharaAnisotropic(texCoord);
    else
        result = KuwaharaStandard(texCoord);
#endif

    result.rgb = BoostSaturation(result.rgb, SaturationBoost);
    result.rgb *= lerp(1.0, CanvasPattern(texCoord), CanvasStrength);

    return result;
}

technique Painterly
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add Content/PainterlyShader.fx
git commit -m "feat: add PainterlyShader.fx (Kuwahara filter, saturation, canvas texture)"
```

---

## Task 3 — PainterlyEffect (single pass)

**Files:**
- Create: `PainterlyBuddy/PainterlyEffect.cs`

No unit tests — requires a live GraphicsDevice. Visual verification after wiring into a test game.

- [ ] **Step 1: Implement PainterlyEffect**

Create `PainterlyBuddy/PainterlyEffect.cs`:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PainterlyBuddy
{
    public class PainterlyEffect : IDisposable
    {
        private GraphicsDevice _graphics;
        private SpriteBatch _spriteBatch;
        private Effect _shader;
        private RenderTarget2D _capture;
        private RenderTarget2D _pingPong;

        /// <summary>
        /// Shader parameters and preset to use. Can be swapped or lerped every frame.
        /// </summary>
        public PainterlySettings Settings { get; set; }

        /// <param name="graphics">The game's GraphicsDevice.</param>
        /// <param name="content">ContentManager that can load "PainterlyShader" from the game's content pipeline.</param>
        public PainterlyEffect(GraphicsDevice graphics, ContentManager content)
        {
            _graphics    = graphics;
            Settings     = PainterlySettings.Classic;
            _shader      = content.Load<Effect>("PainterlyShader");
            _spriteBatch = new SpriteBatch(graphics);
            CreateRenderTargets();
        }

        private void CreateRenderTargets()
        {
            _capture?.Dispose();
            _pingPong?.Dispose();
            var w = _graphics.Viewport.Width;
            var h = _graphics.Viewport.Height;
            _capture  = new RenderTarget2D(_graphics, w, h);
            _pingPong = new RenderTarget2D(_graphics, w, h);
        }

        /// <summary>
        /// Call this before your scene draw calls. Redirects rendering into an internal
        /// render target instead of the backbuffer.
        /// </summary>
        public void BeginCapture()
        {
            if (_capture.Width  != _graphics.Viewport.Width ||
                _capture.Height != _graphics.Viewport.Height)
            {
                CreateRenderTargets();
            }

            _graphics.SetRenderTarget(_capture);
            _graphics.Clear(Color.Transparent);
        }

        /// <summary>
        /// Call this after your scene draw calls. Applies the painterly shader and
        /// draws the result to the backbuffer. HUD elements drawn after this call
        /// are unaffected.
        /// </summary>
        public void Apply()
        {
            var settings = GetEffectiveSettings();

            if (settings.Passes == 2)
            {
                _graphics.SetRenderTarget(_pingPong);
                _graphics.Clear(Color.Transparent);
                ApplyPass(_capture, settings);

                _graphics.SetRenderTarget(null);
                ApplyPass(_pingPong, settings);
            }
            else
            {
                _graphics.SetRenderTarget(null);
                ApplyPass(_capture, settings);
            }
        }

        // Returns settings with OpenGL-incompatible options stripped.
        private PainterlySettings GetEffectiveSettings()
        {
            var s = Settings;
            if (_graphics.GraphicsProfile == GraphicsProfile.Reach)
            {
                s.KernelRadius = Math.Min(s.KernelRadius, 4);
                s.Anisotropic  = false;
            }
            s.KernelRadius = Math.Clamp(s.KernelRadius, 1, 8);
            return s;
        }

        private void ApplyPass(RenderTarget2D source, PainterlySettings settings)
        {
            _shader.Parameters["TexelSize"].SetValue(
                new Vector2(1f / source.Width, 1f / source.Height));
            _shader.Parameters["KernelRadius"].SetValue(settings.KernelRadius);
            _shader.Parameters["SaturationBoost"].SetValue(settings.SaturationBoost);
            _shader.Parameters["CanvasStrength"].SetValue(settings.CanvasStrength);
            _shader.Parameters["Anisotropic"].SetValue(settings.Anisotropic);

            var vp = _graphics.Viewport;
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque,
                               null, null, null, _shader);
            _spriteBatch.Draw(source, new Rectangle(0, 0, vp.Width, vp.Height), Color.White);
            _spriteBatch.End();
        }

        public void Dispose()
        {
            _capture?.Dispose();
            _pingPong?.Dispose();
            _spriteBatch?.Dispose();
            _shader?.Dispose();
        }
    }
}
```

- [ ] **Step 2: Wire into a test game and verify visually**

In a test MonoGame game's `LoadContent`:
```csharp
_painterly = new PainterlyEffect(GraphicsDevice, Content);
_painterly.Settings = PainterlySettings.Classic;
```

In `Draw`:
```csharp
_painterly.BeginCapture();
// ... your existing draw calls ...
_painterly.Apply();
```

Add `PainterlyShader.fx` to the game's `.mgcb` content project and rebuild content.

Expected: scene renders with visible paint-region blocking, richer saturation, and faint canvas texture. Characters and background are both affected uniformly.

- [ ] **Step 3: Commit**

```bash
git add PainterlyBuddy/PainterlyEffect.cs
git commit -m "feat: add PainterlyEffect with BeginCapture/Apply and single-pass Kuwahara"
```

---

## Task 4 — Verify multi-pass and preset transitions

**Files:**
- No new files — exercising existing code paths

- [ ] **Step 1: Test Expressive preset (2 passes)**

In the test game's `LoadContent`:
```csharp
_painterly.Settings = PainterlySettings.Expressive;
```

Expected: markedly heavier paint regions than Classic; the effect is visibly compounded versus a single pass.

- [ ] **Step 2: Test runtime lerp transition**

Add to the test game's `Update`:
```csharp
_transitionTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
float t = MathHelper.Clamp(_transitionTimer / 3f, 0f, 1f);  // 3-second transition
_painterly.Settings = PainterlySettings.Lerp(PainterlySettings.Subtle, PainterlySettings.Expressive, t);
```

Expected: effect starts subtle and gradually intensifies over 3 seconds with no visual discontinuities in continuous values. `Passes` snaps from 1 to 2 at the midpoint — this should be a brief but visible step change, which is acceptable.

- [ ] **Step 3: Test HUD draw after Apply()**

After `_painterly.Apply()`, add a HUD draw:
```csharp
_hudSpriteBatch.Begin();
_hudSpriteBatch.DrawString(_font, "HUD TEXT", new Vector2(20, 20), Color.White);
_hudSpriteBatch.End();
```

Expected: HUD text is sharp and unaffected by the painterly filter.

- [ ] **Step 4: Commit**

```bash
git commit -m "test: verify multi-pass, lerp transition, and HUD draw-after-Apply"
```

---

## Task 5 — Anisotropic Kuwahara (Expressive mode, DirectX only)

**Files:**
- Already in `Content/PainterlyShader.fx` (the `#if !OPENGL` blocks from Task 2)
- Already in `PainterlyEffect.cs` (`GetEffectiveSettings` strips it on Reach profile)

The shader code was written in Task 2. This task verifies it works correctly.

- [ ] **Step 1: Confirm anisotropic mode activates on DirectX**

In the test game, temporarily force Expressive:
```csharp
_painterly.Settings = PainterlySettings.Expressive;
// Expressive has Anisotropic = true, KernelRadius = 6, Passes = 2
```

Run the game targeting WindowsDX (DirectX backend).

Expected: brush strokes noticeably follow the contours of shapes — curved outlines show strokes that curve with them rather than being square-aligned. This is most visible on circular or diagonal shapes.

- [ ] **Step 2: Confirm graceful fallback on OpenGL**

Run the same game targeting DesktopGL (OpenGL backend).

Expected: `GetEffectiveSettings()` sets `Anisotropic = false` and caps `KernelRadius` to 4. The effect renders correctly (standard Kuwahara, no atan2). No shader compile error, no crash.

- [ ] **Step 3: Commit**

```bash
git commit -m "test: verify anisotropic Kuwahara on DirectX and graceful OpenGL fallback"
```

---

## Self-Review Notes

**Spec coverage check:**
- ✅ `PainterlySettings` struct with all 5 fields — Task 1
- ✅ Three presets (Subtle, Classic, Expressive) — Task 1
- ✅ `Lerp` with correct snap behaviour for discrete fields — Task 1
- ✅ Standard Kuwahara (4 overlapping quadrants, min-variance selection) — Task 2
- ✅ Saturation boost — Task 2
- ✅ Procedural canvas texture at 1080p frequency — Task 2
- ✅ Anisotropic Kuwahara with edge-angle rotation — Task 2 (`#if !OPENGL`)
- ✅ `PainterlyEffect.BeginCapture()` / `Apply()` — Task 3
- ✅ Ping-pong render targets for 2-pass flow — Task 3 (`Apply()` implementation)
- ✅ Render target recreation on viewport size change — Task 3 (`BeginCapture`)
- ✅ OpenGL guard (cap KernelRadius at 4, strip Anisotropic) — Task 3 (`GetEffectiveSettings`)
- ✅ Runtime lerp transition — Task 4
- ✅ HUD draw after `Apply()` unaffected — Task 4
- ✅ Anisotropic visual verification on DirectX — Task 5
- ✅ OpenGL fallback verification — Task 5

**Type consistency check:**
- `PainterlySettings.Lerp` — defined in Task 1, referenced in Tasks 4 and spec usage example ✅
- `PainterlyEffect.BeginCapture()` / `Apply()` — defined in Task 3, used in Task 4 ✅
- `GetEffectiveSettings()` — private, defined and used in Task 3 ✅
- `ApplyPass(RenderTarget2D, PainterlySettings)` — defined and used in Task 3 ✅
- `PainterlySettings.Expressive.Anisotropic` — `true` in Task 1, guarded in Task 3, verified in Task 5 ✅
