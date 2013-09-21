using System.Drawing;

namespace RenderBuddy
{
	/// <summary>
	/// This interface wraps up Winform.Image
	/// </summary>
	public class WinFormTexture : ITexture
	{
		#region Properties

		public Image Texture { get; set; }

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
	}
}