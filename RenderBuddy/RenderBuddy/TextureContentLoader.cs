using FilenameBuddy;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace RenderBuddy
{
	public class TextureContentLoader : ITextureLoader
	{
		public TextureInfo LoadImage(IRenderer renderer, Filename textureFile, Filename normalMapFile, Filename colorMaskFile)
		{
			try
			{
				var tex = new TextureInfo()
				{
					Texture = renderer.Content.Load<Texture2D>(textureFile.GetRelPathFileNoExt())
				};

				if (null != normalMapFile && !string.IsNullOrEmpty(normalMapFile.File))
				{
					tex.NormalMap = renderer.Content.Load<Texture2D>(normalMapFile.GetRelPathFileNoExt());
				}

				if (null != colorMaskFile && !string.IsNullOrEmpty(colorMaskFile.File))
				{
					tex.ColorMask = renderer.Content.Load<Texture2D>(colorMaskFile.GetRelPathFileNoExt());
				}

				return tex;
			}
			catch (Exception ex)
			{
				throw new Exception("Error loading an image in TextureContentLoader.LoadImage", ex);
			}
		}
	}
}
