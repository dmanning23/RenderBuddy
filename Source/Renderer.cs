using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using CameraBuddy;

namespace RenderBuddy
{
	public class Renderer
	{
		#region Member Variables

		/// <summary>
		/// the title safe area of the game window
		/// </summary>
		private Rectangle m_TitleSafeArea;

		/// <summary>
		/// the whole game window
		/// </summary>
		private Rectangle m_ScreenRect;

		/// <summary>
		/// My own content manager, so images can be loaded separate from xml
		/// </summary>
		public ContentManager Content { get; private set; }
		private Game m_Game; //needed to initialize content manager

		//the graphics card device manager
		private GraphicsDevice m_Graphics;

		//sprite batch being used
		public SpriteBatch SpriteBatch { get; private set; }

		/// <summary>
		/// The camera we are going to use!
		/// </summary>
		public Camera Camera { get; private set; }

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
		public Renderer(Game GameReference)
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

			//set up the camera
			Camera = new Camera();
			Camera.WorldBoundary = new Rectangle(-2000, -1000, 4000, 2000);
		}

		/// <summary>
		/// Reload all the graphics content
		/// </summary>
		public void LoadContent(GraphicsDevice myGraphics, Rectangle screenRect, Rectangle titleSafeRect)
		{
			//grab all the member variables
			Debug.Assert(null != myGraphics);
			m_Graphics = myGraphics;
			m_ScreenRect = screenRect;
			m_TitleSafeArea = titleSafeRect;

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

			//Setup all the rectangles used by the camera
			Camera.SetScreenRects(myGraphics.Viewport.Bounds, myGraphics.Viewport.TitleSafeArea);
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

		public void Draw(
			Texture2D iImageID,
			Vector2 Position,
			Color rColor, 
			float fRotation,
			bool bFlip,
			float fScale)
		{
			SpriteBatch.Draw(
				iImageID,
				Position,
				null,
				rColor,
				fRotation,
				Vector2.Zero,
				fScale,
				(bFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None),
				0.0f);
		}

		public void Draw(
			Texture2D iImageID,
			Rectangle Destination,
			Color rColor,
			float fRotation,
			bool bFlip)
		{
			SpriteBatch.Draw(
				iImageID,
				Destination,
				null,
				rColor,
				fRotation,
				Vector2.Zero,
				(bFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None),
				0.0f);
		}

		public void SpriteBatchBegin(BlendState myBlendState, Matrix translation)
		{
			////update all the matrices of the camera before we start drawing
			//_camera.BeginScene(false);
			//_camera.TranslationMatrix * Resolution.TransformationMatrix()

			SpriteBatch.Begin(SpriteSortMode.Deferred, //TODO: switch this to deferred sorting?
				myBlendState, 
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
		public void SpriteBatchEnd()
		{
			SpriteBatch.End();
		}

		#endregion
	}
}