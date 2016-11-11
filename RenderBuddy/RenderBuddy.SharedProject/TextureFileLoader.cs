using FilenameBuddy;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace RenderBuddy
{
	public class TextureFileLoader : ITextureLoader
	{
		public TextureInfo LoadImage(IRenderer renderer, Filename textureFile, Filename normalMapFile, Filename colorMaskFile)
		{
			var tex = new TextureInfo();

			using (var textureStream = File.OpenRead(textureFile.File))
			{
				tex.Texture = Texture2D.FromStream(renderer.Graphics, textureStream);
			}

			if (null != normalMapFile && !string.IsNullOrEmpty(normalMapFile.File))
			{
				using (var normalStream = File.OpenRead(normalMapFile.File))
				{
					tex.NormalMap = Texture2D.FromStream(renderer.Graphics, normalStream);
				}
			}

			if (null != colorMaskFile && !string.IsNullOrEmpty(colorMaskFile.File))
			{
				using (var colorStream = File.OpenRead(colorMaskFile.File))
				{
					tex.ColorMask = Texture2D.FromStream(renderer.Graphics, colorStream);
				}
			}

			return tex;
		}
	}
}
