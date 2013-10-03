using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using CameraBuddy;
using BasicPrimitiveBuddy;

namespace RenderBuddy.Tests
{
	public class TestRenderer : IRenderer
	{
		#region Member Variables

		#endregion //Member Variables

		#region Properties

		/// <summary>
		/// The camera we are going to use!
		/// </summary>
		public Camera Camera { get; private set; }

		/// <summary>
		/// thing for rendering primitives.
		/// </summary>
		/// <value>The primitive.</value>
		public IBasicPrimitive Primitive { get; private set; }

		#endregion

		#region Initialization

		/// <summary>
		/// Hello, standard constructor!
		/// </summary>
		/// <param name="GameReference">Reference to the game engine</param>
		public TestRenderer()
		{
			Camera = new Camera();
			Primitive = new TestBasicPrimitive();
		}

		#endregion

		#region Methods

		public void Draw(ITexture image, Vector2 Position, Color rColor, float fRotation, bool bFlip, float fScale)
		{
		}

		public void Draw(ITexture image, Rectangle Destination, Color rColor, float fRotation, bool bFlip)
		{
		}

		public ITexture LoadImage(string file)
		{
			return new TestTexture(file);
		}

		#endregion
	}
}