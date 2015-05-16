using BasicPrimitiveBuddy;
using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace RenderBuddy
{
	public class XNARenderer : RendererBase
	{
		#region Member Variables

		/// <summary>
		/// needed to initialize content manager
		/// </summary>
		private Game m_Game;

		/// <summary>
		/// the graphics card device manager
		/// </summary>
		private GraphicsDevice m_Graphics;

		/// <summary>
		/// sprite batch being used
		/// </summary>
		/// <value>The sprite batch.</value>
		public SpriteBatch SpriteBatch { get; private set; }

		#endregion //Member Variables

		#region Properties

		public GraphicsDevice Graphics
		{
			get { return m_Graphics; }
		}

		#endregion

		#region Initialization

		/// <summary>
		/// Hello, standard constructor!
		/// </summary>
		/// <param name="GameReference">Reference to the game engine</param>
		public XNARenderer(Game GameReference)
		{
			//set up the content manager
			Debug.Assert(null != GameReference);
			m_Game = GameReference;

			//set up the content manager
			Debug.Assert(null != m_Game);
			Debug.Assert(null == Content);
			Content = new ContentManager(m_Game.Services, "Content");

			//set up all the stuff
			m_Graphics = null;
			SpriteBatch = null;
		}

		/// <summary>
		/// Reload all the graphics content
		/// </summary>
		public void LoadContent(GraphicsDevice myGraphics)
		{
			//grab all the member variables
			Debug.Assert(null != myGraphics);
			m_Graphics = myGraphics;

			SpriteBatch = new SpriteBatch(m_Graphics);

			//setup all the rendering stuff
			Debug.Assert(null != m_Graphics);
			Debug.Assert(null != m_Graphics.BlendState);

			BlendState myBlendState = new BlendState();
			myBlendState.AlphaSourceBlend = Blend.SourceAlpha;
			myBlendState.ColorSourceBlend = Blend.SourceAlpha;
			myBlendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
			myBlendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
			m_Graphics.BlendState = myBlendState;

			Primitive = new XnaBasicPrimitive(myGraphics, SpriteBatch);
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

		public override void Draw(ITexture image, Vector2 Position, Color rColor, float fRotation, bool bFlip, float fScale)
		{
			XNATexture tex = image as XNATexture;
			SpriteBatch.Draw(
				tex.Texture,
				Position,
				null,
				rColor,
				fRotation,
				Vector2.Zero,
				fScale,
				(bFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None),
				0.0f);
		}

		public override void Draw(ITexture image, Rectangle Destination, Color rColor, float fRotation, bool bFlip)
		{
			XNATexture tex = image as XNATexture;
			SpriteBatch.Draw(
				tex.Texture,
				Destination,
				null,
				rColor,
				fRotation,
				Vector2.Zero,
				(bFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None),
				0.0f);
		}

		public override ITexture LoadImage(Filename file)
		{
			XNATexture tex = new XNATexture()
			{
				Texture = Content.Load<Texture2D>(file.GetRelPathFileNoExt())
			};

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

		/// <summary>
		/// 
		/// </summary>
		/// <param name=""></param>
		public override void SpriteBatchEnd()
		{
			SpriteBatch.End();
		}

		#endregion
	}
}