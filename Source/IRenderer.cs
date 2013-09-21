using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using CameraBuddy;
using BasicPrimitiveBuddy;

namespace RenderBuddy
{
	public interface IRenderer
	{
		#region Properties

		IBasicPrimitive Primitive { get; }

		Camera Camera { get; }

		#endregion //Properties

		#region Methods

		void Draw(ITexture image, Vector2 Position, Color rColor, float fRotation, bool bFlip, float fScale);

		void Draw(ITexture image, Rectangle Destination, Color rColor, float fRotation, bool bFlip);

		ITexture LoadImage(string file);

		#endregion //Methods
	}
}