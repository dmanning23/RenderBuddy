using CameraBuddy;
using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PrimitiveBuddy;
using System.Collections.Generic;
using System.Diagnostics;

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
		private Effect _animationEffect;

		private EffectParameterCollection _effectsParams;

		/// <summary>
		/// My own content manager, so images can be loaded separate from xml
		/// </summary>
		public ContentManager Content { get; protected set; }

		/// <summary>
		/// The camera we are going to use!
		/// </summary>
		public Camera Camera { get; protected set; }

		/// <summary>
		/// thing for rendering primitives.
		/// </summary>
		/// <value>The primitive.</value>
		public Primitive Primitive { get; protected set; }

		/// <summary>
		/// The object used to load textures
		/// </summary>
		public ITextureLoader TextureLoader { private get; set; }

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

		public List<DirectionLight> DirectionLights { get; private set; }

		public List<PointLight> PointLights { get; private set; }

		public const int MaxDirectionLights = 4;
		public const int MaxPointLights = 32;

		private Vector3[] _directionLights = new Vector3[MaxDirectionLights];
		private Vector3[] _directionLightColors = new Vector3[MaxDirectionLights];

		#endregion

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
			Debug.Assert(null != game);
			Debug.Assert(null != content);
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
		}

		/// <summary>
		/// Reload all the graphics content
		/// </summary>
		public void LoadContent(GraphicsDevice graphics)
		{
			//grab all the member variables
			Debug.Assert(null != graphics);
			Graphics = graphics;

			SpriteBatch = new SpriteBatch(Graphics);

			//setup all the rendering stuff
			Debug.Assert(null != Graphics);
			Debug.Assert(null != Graphics.BlendState);

			BlendState myBlendState = new BlendState();
			myBlendState.AlphaSourceBlend = Blend.SourceAlpha;
			myBlendState.ColorSourceBlend = Blend.SourceAlpha;
			myBlendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
			myBlendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
			Graphics.BlendState = myBlendState;

			_animationEffect = Content.Load<Effect>(@"AnimationBuddyShader");
			_effectsParams = _animationEffect.Parameters;
			_effectsParams["AmbientColor"].SetValue(AmbientColor.ToVector3());

			Primitive = new Primitive(graphics, SpriteBatch);
		}

		public void ClearLights()
		{
			DirectionLights.Clear();
			PointLights.Clear();
		}

		public void AddDirectionalLight(Vector3 direction, Color color)
		{
			if (DirectionLights.Count < MaxDirectionLights)
			{
				DirectionLights.Add(new DirectionLight(direction, color));
			}
		}

		public void AddPointLight(Vector2 position, float radius, float brightness, Color color)
		{
			if (PointLights.Count < MaxPointLights)
			{
				PointLights.Add(new PointLight(position, radius, brightness, color));
			}
		}

		#endregion

		#region Methods

		public void Draw(TextureInfo image, Vector2 position, Color primaryColor, Color secondaryColor, float rotation, bool isFlipped, float scale)
		{
			Debug.Assert(null != image);
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
				0.0f);
		}

		public void Draw(TextureInfo image, Rectangle destination, Color primaryColor, Color secondaryColor, float rotation, bool isFlipped)
		{
			Debug.Assert(null != image);
			SetEffectParams(image, secondaryColor, rotation, isFlipped);

			SpriteBatch.Draw(
				image.Texture,
				destination,
				image.SourceRectangle.HasValue ? image.SourceRectangle : null,
				primaryColor,
				rotation,
				Vector2.Zero,
				(isFlipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None),
				0.0f);
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

			for (var i = 0; i < DirectionLights.Count; i++)
			{
				_directionLights[i] = DirectionLights[i].Direction;
				_directionLightColors[i] = DirectionLights[i].Color.ToVector3();
			}

			_effectsParams["NumberOfDirectionLights"].SetValue(DirectionLights.Count);
			_effectsParams["DirectionLights"].SetValue(_directionLights);
			_effectsParams["DirectionLightColors"].SetValue(_directionLightColors);
		}

		public TextureInfo LoadImage(Filename textureFile, Filename normalMapFile = null, Filename colorMaskFile = null)
		{
			return TextureLoader.LoadImage(this, textureFile, normalMapFile, colorMaskFile);
		}

		public void SpriteBatchBegin(BlendState blendState, Matrix translation)
		{
			SpriteBatchBegin(SpriteSortMode.Immediate, blendState, translation);
		}

		public void SpriteBatchBegin(SpriteSortMode sortmode, BlendState blendState, Matrix translation)
		{
			SpriteBatch.Begin(sortmode,
				blendState,
				null,
				null,
				RasterizerState.CullNone,
				_animationEffect,
				translation);
		}

		public void SpriteBatchEnd()
		{
			SpriteBatch.End();
		}

		public void DrawCameraInfo()
		{
			//draw the center point
			Primitive.Point(Camera.Origin, Color.Red);
		}

		#endregion
	}
}