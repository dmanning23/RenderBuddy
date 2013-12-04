using BasicPrimitiveBuddy;
using CameraBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace RenderBuddy
{
	public interface IRenderer
	{
		#region Properties

		/// <summary>
		/// My own content manager, so images can be loaded separate from xml
		/// </summary>
		ContentManager Content { get; }

		IBasicPrimitive Primitive { get; }

		Camera Camera { get; }

		#endregion //Properties

		#region Methods

		void Draw(ITexture image, Vector2 Position, Color rColor, float fRotation, bool bFlip, float fScale);

		void Draw(ITexture image, Rectangle Destination, Color rColor, float fRotation, bool bFlip);

		ITexture LoadImage(string file);

		void DrawCameraInfo();

		/// <summary>
		/// Unload all the graphics content
		/// </summary>
		void UnloadGraphicsContent();

		/// <summary>
		/// called at the start of the draw loop
		/// </summary>
		/// <param name="myBlendState"></param>
		/// <param name="translation"></param>
		void SpriteBatchBegin(BlendState myBlendState, Matrix translation);

		/// <summary>
		/// called at the end of the draw loop
		/// </summary>
		/// <param name=""></param>
		void SpriteBatchEnd();

		#endregion //Methods
	}
}