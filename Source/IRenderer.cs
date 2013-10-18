using BasicPrimitiveBuddy;
using CameraBuddy;
using Microsoft.Xna.Framework;

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