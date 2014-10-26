using BasicPrimitiveBuddy;
using CameraBuddy;
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
		public RendererBase()
		{
			//set up the camera
			Camera = new Camera();
			Camera.WorldBoundary = new Rectangle(-2000, -1000, 4000, 2000);
		}

		/// <summary>
		/// Unload all the graphics content
		/// </summary>
		public virtual void UnloadGraphicsContent()
		{
		}

		#endregion

		#region Methods

		public abstract void Draw(ITexture image, Vector2 Position, Color rColor, float fRotation, bool bFlip, float fScale);

		public abstract void Draw(ITexture image, Rectangle Destination, Color rColor, float fRotation, bool bFlip);

		public abstract ITexture LoadImage(string file);

		public virtual void SpriteBatchBegin(BlendState myBlendState, Matrix translation)
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