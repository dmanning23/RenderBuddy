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
				//width of cortchfront.png
				return 129;
			}
		}

		public int Height
		{
			get
			{
				//height of cortchfront.png
				return 116;
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