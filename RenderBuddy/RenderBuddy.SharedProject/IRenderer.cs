using CameraBuddy;
using FilenameBuddy;
using GameTimer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using PrimitiveBuddy;
using System;
using System.Collections.Generic;

namespace RenderBuddy
{
	public interface IRenderer : IDisposable
	{
		#region Properties

		Effect AnimationEffect { get; }

		SpriteBatch SpriteBatch { get; }

		GraphicsDevice Graphics { get; set; }

		/// <summary>
		/// My own content manager, so images can be loaded separate from xml
		/// </summary>
		ContentManager Content { get; }

		ITextureLoader TextureLoader { set; }

		Primitive Primitive { get; }

		ICamera Camera { get; }

		Color AmbientColor { get; set; }

		List<DirectionLight> DirectionLights { get; }

		List<PointLight> PointLights { get; }

		#endregion //Properties

		#region Methods

		void ClearLights();

		void AddDirectionalLight(Vector3 direction, Color color);

		void AddPointLight(Vector3 position, float brightness, Color color);

		void Update(GameTime gameTime);

		void Update(GameClock gameTime);

		void Draw(TextureInfo image, Vector2 position, Color primaryColor, Color secondaryColor, float rotation, bool isFlipped, float scale, float layer);

		void Draw(TextureInfo image, Rectangle destination, Color primaryColor, Color secondaryColor, float rotation, bool isFlipped, float layer);

		void LoadContent(GraphicsDevice graphics);

		void UnloadContent();

		TextureInfo LoadImage(Filename textureFile, Filename normalMapFile = null, Filename colorMaskFile = null);

		void DrawCameraInfo();

		/// <summary>
		/// called at the start of the draw loop
		/// </summary>
		void SpriteBatchBegin(BlendState blendState, Matrix translation, SpriteSortMode sortmode = SpriteSortMode.Immediate);

		void SpriteBatchBeginNoEffect(BlendState blendState, Matrix translation, SpriteSortMode sortmode = SpriteSortMode.Immediate);

		/// <summary>
		/// called at the end of the draw loop
		/// </summary>
		/// <param name=""></param>
		void SpriteBatchEnd();

		#endregion //Methods
	}
}