using CameraBuddy;
using FilenameBuddy;
using GameTimer;
using MatrixExtensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PrimitiveBuddy;
using ResolutionBuddy;
using System;
using System.Collections.Generic;

namespace RenderBuddy
{
	public class Renderer : IRenderer
	{
		#region Properties

		/// <summary>
		/// sprite batch being used
		/// </summary>
		/// <value>The sprite batch.</value>
		public SpriteBatch SpriteBatch { get; private set; }

		/// <summary>
		/// the graphics card device manager
		/// </summary>
		public GraphicsDevice Graphics { get; set; }

		/// <summary>
		/// Shader to draw the texture, light correctly using the supplied normal map
		/// </summary>
		public Effect AnimationEffect { get; private set; }

		private EffectParameterCollection _effectsParams;

		/// <summary>
		/// My own content manager, so images can be loaded separate from xml
		/// </summary>
		public ContentManager Content { get; protected set; }

		/// <summary>
		/// The camera we are going to use!
		/// </summary>
		public ICamera Camera { get; protected set; }

		/// <summary>
		/// thing for rendering primitives.
		/// </summary>
		/// <value>The primitive.</value>
		public Primitive Primitive { get; protected set; }

		/// <summary>
		/// The object used to load textures
		/// </summary>
		public ITextureLoader TextureLoader { private get; set; }

		public GameClock Clock { get; set; }

		private Matrix _matrix;

		#region Ambient Light

		private Color _ambientColor;
		public Color AmbientColor
		{
			get { return _ambientColor; }
			set
			{
				_ambientColor = value;
				if (null != _effectsParams)
				{
					_effectsParams["AmbientColor"].SetValue(AmbientColor.ToVector3());
				}
			}
		}

		#endregion //Ambient Light

		#region Direction Lights

		public List<DirectionLight> DirectionLights { get; private set; }

		public const int MaxDirectionLights = 3;

		private Vector3[] _directionLights = new Vector3[MaxDirectionLights];
		private Vector4[] _directionLightColors = new Vector4[MaxDirectionLights];

		#endregion //Direction Lights

		#region Point Lights

		public List<PointLight> PointLights { get; private set; }

#if ANDROID || __IOS__
		public const int MaxPointLights = 32;
#else
		public const int MaxPointLights = 4;
#endif

		private Vector3[] _pointLights = new Vector3[MaxPointLights];
		private Vector3[] _pointLightColors = new Vector3[MaxPointLights];
		private float[] _pointLightBrightness = new float[MaxPointLights];

		#endregion //Point Lights

		#endregion //Properties

		#region Initialization

		/// <summary>
		/// Hello, standard constructor!
		/// </summary>
		/// <param name="game">Reference to the game engine</param>
		public Renderer(Game game, ContentManager content)
		{
			AmbientColor = new Color(.35f, .35f, .35f);

			DirectionLights = new List<DirectionLight>();

			//Add a default directional light
			DirectionLights.Add(new DirectionLight(new Vector3(0f, -1f, .2f), new Color(1f, 1f, .75f)));

			PointLights = new List<PointLight>();

			//set up the content manager
			Content = content;
			TextureLoader = new TextureContentLoader();

			//set up all the stuff
			Graphics = null;
			SpriteBatch = null;

			//set up the camera
			Camera = new Camera()
			{
				WorldBoundary = new Rectangle(-2000, -1000, 4000, 2000)
			};

			Clock = new GameClock();
		}

		/// <summary>
		/// Reload all the graphics content
		/// </summary>
		public void LoadContent(GraphicsDevice graphics)
		{
			//grab all the member variables
			Graphics = graphics;

			SpriteBatch = new SpriteBatch(Graphics);

			//setup all the rendering stuff
			BlendState myBlendState = new BlendState();
			myBlendState.AlphaSourceBlend = Blend.SourceAlpha;
			myBlendState.ColorSourceBlend = Blend.SourceAlpha;
			myBlendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
			myBlendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
			Graphics.BlendState = myBlendState;

			AnimationEffect = Content.Load<Effect>(@"AnimationBuddyShader");
			_effectsParams = AnimationEffect.Parameters;
			_effectsParams["AmbientColor"].SetValue(AmbientColor.ToVector3());

			Primitive = new Primitive(graphics, SpriteBatch);
		}

		public void UnloadContent()
		{
			SpriteBatch?.Dispose();
			SpriteBatch = null;

			Primitive?.Dispose();
			Primitive = null;
		}

		public void Dispose()
		{
			UnloadContent();
		}

		#endregion //Initialization

		#region Methods

		public void ClearLights()
		{
			DirectionLights.Clear();
			PointLights.Clear();
		}

		public DirectionLight AddDirectionalLight(Vector3 direction, Color color)
		{
			if (DirectionLights.Count < MaxDirectionLights)
			{
				var light = new DirectionLight(direction, color);
				DirectionLights.Add(light);
				return light;
			}
			return null;
		}

		public void AddPointLight(PointLight pointLight)
		{
			if (PointLights.Count < MaxPointLights)
			{
				PointLights.Add(pointLight);
			}
		}

		public PointLight AddPointLight(Vector3 position, float brightness, Color color)
		{
			var light = new PointLight(position, brightness, color);
			AddPointLight(light);
			return light;
		}

		public void Update(GameTime gameTime)
		{
			Clock.Update(gameTime);
			UpdateLights();
		}

		public void Update(GameClock gameTime)
		{
			Clock.Update(gameTime);
			UpdateLights();
		}

		private void UpdateLights()
		{
			//clean up any expired lights
			int i = 0;
			while (i < DirectionLights.Count)
			{
				DirectionLights[i].Update(Clock);
				if (DirectionLights[i].IsDead)
				{
					DirectionLights.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}

			i = 0;
			while (i < PointLights.Count)
			{
				PointLights[i].Update(Clock);
				if (PointLights[i].IsDead)
				{
					PointLights.RemoveAt(i);
				}
				else
				{
					i++;
				}
			}
		}

		public void Draw(TextureInfo image, Vector2 position, Color primaryColor, Color secondaryColor, float rotation, bool isFlipped, float scale, float layer)
		{
			var tex = image as TextureInfo;
			SetEffectParams(tex, secondaryColor, rotation, isFlipped);

			SpriteBatch.Draw(
				tex.Texture,
				position,
				image.SourceRectangle.HasValue ? image.SourceRectangle : null,
				primaryColor,
				rotation,
				Vector2.Zero,
				scale,
				(isFlipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None),
				layer);
		}

		public void Draw(TextureInfo image, Rectangle destination, Color primaryColor, Color secondaryColor, float rotation, bool isFlipped, float layer)
		{
			SetEffectParams(image, secondaryColor, rotation, isFlipped);

			SpriteBatch.Draw(
				image.Texture,
				destination,
				image.SourceRectangle.HasValue ? image.SourceRectangle : null,
				primaryColor,
				rotation,
				Vector2.Zero,
				(isFlipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None),
				layer);
		}

		/// <summary>
		/// Setup the effect params for rendering
		/// </summary>
		/// <param name="image"></param>
		/// <param name="secondaryColor"></param>
		/// <param name="rotation"></param>
		/// <param name="isFlipped"></param>
		private void SetEffectParams(TextureInfo image, Color secondaryColor, float rotation, bool isFlipped)
		{
			_effectsParams["NormalTexture"].SetValue(image.NormalMap);
			_effectsParams["HasNormal"].SetValue(image.HasNormal);
			_effectsParams["Rotation"].SetValue(rotation);
			_effectsParams["ColorMaskTexture"].SetValue(image.ColorMask);
			_effectsParams["HasColorMask"].SetValue(image.HasColorMask);
			_effectsParams["ColorMask"].SetValue(secondaryColor.ToVector4());
			_effectsParams["FlipHorizontal"].SetValue(isFlipped);

			//Add the direction lights
			for (var i = 0; i < DirectionLights.Count; i++)
			{
				_directionLights[i] = DirectionLights[i].Direction;
				_directionLightColors[i] = DirectionLights[i].Color.ToVector4();
			}
			_effectsParams["NumberOfDirectionLights"].SetValue(DirectionLights.Count);
			_effectsParams["DirectionLights"].SetValue(_directionLights);
			_effectsParams["DirectionLightColors"].SetValue(_directionLightColors);

			//Add the point lights
			for (var i = 0; i < PointLights.Count; i++)
			{
				var pos = MatrixExt.Multiply(_matrix, new Vector2(PointLights[i].Position.X, PointLights[i].Position.Y));
				_pointLights[i] = new Vector3(pos.X, pos.Y, PointLights[i].Position.Z);
				_pointLightColors[i] = PointLights[i].Color.ToVector3();
				_pointLightBrightness[i] = PointLights[i].Brightness * Camera.Scale;
			}
			_effectsParams["NumberOfPointLights"].SetValue(PointLights.Count);
			_effectsParams["PointLights"].SetValue(_pointLights);
			_effectsParams["PointLightColors"].SetValue(_pointLightColors);
			_effectsParams["PointLightBrightness"].SetValue(_pointLightBrightness);
		}

		public TextureInfo LoadImage(Filename textureFile, Filename normalMapFile = null, Filename colorMaskFile = null)
		{
			try
			{
				return TextureLoader.LoadImage(this, textureFile, normalMapFile, colorMaskFile);
			}
			catch (Exception ex)
			{
				throw new Exception($"There was an error in {textureFile.GetRelFilename()}", ex);
			}
		}

		public void SpriteBatchBegin(BlendState blendState, Matrix translation, SpriteSortMode sortmode = SpriteSortMode.Immediate)
		{
			_matrix = translation;

			SpriteBatch.Begin(sortmode,
				blendState,
				null,
				null,
				RasterizerState.CullNone,
				AnimationEffect,
				translation);
		}

		public void SpriteBatchBeginNoEffect(BlendState blendState, Matrix translation, SpriteSortMode sortmode = SpriteSortMode.Immediate)
		{
			_matrix = translation;

			SpriteBatch.Begin(sortmode,
				blendState,
				null,
				null,
				RasterizerState.CullNone,
				null,
				translation);
		}

		public void SpriteBatchEnd()
		{
			SpriteBatch.End();
		}

		public void DrawCameraInfo()
		{
			//draw the center point
			Primitive.Point(Camera.Center, Color.Red);
		}

		#endregion //Methods
	}
}