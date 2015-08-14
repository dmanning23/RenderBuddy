using BasicPrimitiveBuddy;
using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace RenderBuddy
{
	public class XnaRenderer : RendererBase
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

		#endregion

		#region Initialization

		/// <summary>
		/// Hello, standard constructor!
		/// </summary>
		/// <param name="game">Reference to the game engine</param>
		public XnaRenderer(Game game)
		{
			//set up the content manager
			Debug.Assert(null != game);
			Debug.Assert(null == Content);
			Content = new ContentManager(game.Services, "Content");

			//set up all the stuff
			Graphics = null;
			SpriteBatch = null;
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

			Primitive = new XnaBasicPrimitive(graphics, SpriteBatch);
		}

		/// <summary>
		/// Unload all the graphics content
		/// </summary>
		public override void UnloadGraphicsContent()
		{
			//unload the bitmaps
			Content.Unload();
		}

		#endregion

		#region Methods

		public override void Draw(ITexture image, Vector2 position, Color primaryColor, Color secondaryColor, float rotation, bool isFlipped, float scale)
		{
			Debug.Assert(null != image);
			var tex = image as XnaTexture;
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

		public override void Draw(ITexture image, Rectangle destination, Color primaryColor, Color secondaryColor, float rotation, bool isFlipped)
		{
			Debug.Assert(null != image);
			var tex = image as XnaTexture;
			SpriteBatch.Draw(
				tex.Texture,
				destination,
				null,
				primaryColor,
				rotation,
				Vector2.Zero,
				(isFlipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None),
				0.0f);
		}

		public override ITexture LoadImage(Filename textureFile, Filename normalMapFile = null, Filename colorMaskFile = null)
		{
			var tex = new XnaTexture()
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

		public override void SpriteBatchBegin(BlendState blendState, Matrix translation)
		{
			SpriteBatchBegin(SpriteSortMode.Immediate, blendState, translation);
		}

		public override void SpriteBatchBegin(SpriteSortMode sortmode, BlendState blendState, Matrix translation)
		{
			SpriteBatch.Begin(sortmode,
				blendState,
				null,
				null,
				RasterizerState.CullNone,
				null,
				translation);
		}

		public override void SpriteBatchEnd()
		{
			SpriteBatch.End();
		}

		#endregion
	}
}