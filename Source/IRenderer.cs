using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using CameraBuddy;
using BasicPrimitiveBuddy;

namespace RenderBuddy
{
	public interface IRenderer<T>
	{
		#region Properties

		IBasicPrimitive Primitive { get; }

		Camera Camera { get; }

		#endregion //Properties

		#region Methods

		void Draw(T image, Vector2 Position, Color rColor, float fRotation, bool bFlip, float fScale);

		void Draw(T image, Rectangle Destination, Color rColor, float fRotation, bool bFlip);

		T LoadImage(string file);

		#endregion //Methods
	}
}