using CameraBuddy;
using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PrimitiveBuddy;

namespace RenderBuddy
{
	public interface IRenderer
	{
		#region Properties

		SpriteBatch SpriteBatch { get; }

		GraphicsDevice Graphics { get; set; }

		/// <summary>
		/// My own content manager, so images can be loaded separate from xml
		/// </summary>
		ContentManager Content { get; }

		Primitive Primitive { get; }

		Camera Camera { get; }

		#endregion //Properties

		#region Methods

		void Draw(TextureInfo image, Vector2 position, Color primaryColor, Color secondaryColor, float rotation, bool isFlipped, float scale);

		void Draw(TextureInfo image, Rectangle destination, Color primaryColor, Color secondaryColor, float rotation, bool isFlipped);

		void LoadContent(GraphicsDevice graphics);

		TextureInfo LoadImage(Filename textureFile, Filename normalMapFile = null, Filename colorMaskFile = null);

		void DrawCameraInfo();

		void SpriteBatchBegin(SpriteSortMode sortmode, BlendState blendState, Matrix translation);

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