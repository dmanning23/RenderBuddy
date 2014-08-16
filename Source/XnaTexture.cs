using Microsoft.Xna.Framework.Graphics;

namespace RenderBuddy
{
	/// <summary>
	/// This interface wraps up XNA.Texture2D
	/// </summary>
	public class XNATexture : ITexture
	{
		#region Properties

		public Texture2D Texture { get; set; }

		public int Width
		{
			get
			{
				return Texture.Width;
			}
		}

		public int Height
		{
			get
			{
				return Texture.Height;
			}
		}

		#endregion //Properties

		#region Methods

		public XNATexture()
		{
		}

		public XNATexture(Texture2D tex)
		{
			Texture = tex;
		}

		#endregion Methods
	}
}