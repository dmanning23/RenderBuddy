using BasicPrimitiveBuddy;
using CameraBuddy;
using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RenderBuddy
{
	public abstract class RendererBase : IRenderer
	{
		#region Properties

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
		public IBasicPrimitive Primitive { get; protected set; }

		#endregion //Properties

		#region Initialization

		/// <summary>
		/// Hello, standard constructor!
		/// </summary>
		protected RendererBase()
		{
			//set up the camera
			Camera = new Camera()
			{
				WorldBoundary = new Rectangle(-2000, -1000, 4000, 2000)
			};
		}

		/// <summary>
		/// Unload all the graphics content
		/// </summary>
		public virtual void UnloadGraphicsContent()
		{
		}

		#endregion

		#region Methods

		public abstract void Draw(ITexture image, Vector2 position, Color primaryColor, Color secondaryColor, float rotation,
			bool isFlipped, float scale);

		public abstract void Draw(ITexture image, Rectangle destination, Color primaryColor, Color secondaryColor,
			float rotation, bool isFlipped);

		public abstract ITexture LoadImage(Filename textureFile, Filename normalMapFile = null, Filename colorMaskFile = null);

		public virtual void SpriteBatchBegin(BlendState blendState, Matrix translation)
		{
		}

		public virtual void SpriteBatchBegin(SpriteSortMode sortMode, BlendState blendState, Matrix translation)
		{
		}

		public virtual void SpriteBatchEnd()
		{
		}

		public void DrawCameraInfo()
		{
			//draw the center point
			Primitive.Point(Camera.Origin, Color.Red);
		}

		#endregion
	}
}