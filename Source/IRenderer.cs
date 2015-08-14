using BasicPrimitiveBuddy;
using CameraBuddy;
using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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

		void Draw(ITexture image, Vector2 position, Color primaryColor, Color secondaryColor, float rotation, bool isFlipped, float scale);

		void Draw(ITexture image, Rectangle destination, Color primaryColor, Color secondaryColor, float rotation, bool isFlipped);

		ITexture LoadImage(Filename textureFile, Filename normalMapFile = null, Filename colorMaskFile = null);

		void DrawCameraInfo();

		/// <summary>
		/// Unload all the graphics content
		/// </summary>
		void UnloadGraphicsContent();

		/// <summary>
		/// called at the start of the draw loop
		/// </summary>
		/// <param name="blendState"></param>
		/// <param name="translation"></param>
		void SpriteBatchBegin(BlendState blendState, Matrix translation);

		/// <summary>
		/// called at the end of the draw loop
		/// </summary>
		/// <param name=""></param>
		void SpriteBatchEnd();

		#endregion //Methods
	}
}