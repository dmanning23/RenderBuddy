using CameraBuddy;
using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PrimitiveBuddy;
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

		private EffectParameterCollection _efectsParams;

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

		#endregion

		#region Initialization

		/// <summary>
		/// Hello, standard constructor!
		/// </summary>
		/// <param name="game">Reference to the game engine</param>
		public Renderer(Game game)
		{
			//set up the content manager
			Debug.Assert(null != game);
			Debug.Assert(null == Content);
			Content = new ContentManager(game.Services, "Content");

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

			_animationEffect = Content.Load<Effect>(@"Shaders\AnimationBuddyShader");
			_efectsParams = _animationEffect.Parameters;

			_efectsParams["LightDirection"].SetValue(new Vector3(0f, 1f, .2f));
			_efectsParams["AmbientColor"].SetValue(new Vector3(.45f, .45f, .45f));
			_efectsParams["LightColor"].SetValue(new Vector3(1f, 1f, 1f));

			Primitive = new Primitive(graphics, SpriteBatch);
		}

		/// <summary>
		/// Unload all the graphics content
		/// </summary>
		public void UnloadGraphicsContent()
		{
			//unload the bitmaps
			Content.Unload();
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
				null,
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
				null,
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
			_efectsParams["NormalTexture"].SetValue(image.NormalMap);
			_efectsParams["HasNormal"].SetValue(image.HasNormal);
			_efectsParams["Rotation"].SetValue(rotation);
			_efectsParams["ColorMaskTexture"].SetValue(image.ColorMask);
			_efectsParams["HasColorMask"].SetValue(image.HasColorMask);
			_efectsParams["ColorMask"].SetValue(secondaryColor.ToVector4());
			_efectsParams["FlipHorizontal"].SetValue(isFlipped);
		}

		public TextureInfo LoadImage(Filename textureFile, Filename normalMapFile = null, Filename colorMaskFile = null)
		{
			var tex = new TextureInfo()
			{
				Texture = Content.Load<Texture2D>(textureFile.GetRelPathFileNoExt())
			};

			if (null != normalMapFile)
			{
				tex.NormalMap = Content.Load<Texture2D>(normalMapFile.GetRelPathFileNoExt());
			}

			if (null != colorMaskFile)
			{
				tex.ColorMask = Content.Load<Texture2D>(colorMaskFile.GetRelPathFileNoExt());
			}

			return tex;
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