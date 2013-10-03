using Microsoft.Xna.Framework.Graphics;
using RenderBuddy;

namespace RenderBuddy.Tests
{
	/// <summary>
	/// This thing is a texture with a string. used only for testing
	/// </summary>
	public class TestTexture : ITexture
	{
		public string Name { get; set; }

		public int Width
		{
			get
			{
				return 256;
			}
		}

		public int Height
		{
			get
			{
				return 256;
			}
		}
		
		public TestTexture(string strName)
		{
			Name = strName;
		}

		public TestTexture()
		{
		}
	}
}