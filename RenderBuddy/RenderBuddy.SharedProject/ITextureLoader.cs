using FilenameBuddy;
using Microsoft.Xna.Framework.Content;

namespace RenderBuddy
{
	public interface ITextureLoader
    {
		TextureInfo LoadImage(IRenderer renderer, Filename textureFile, Filename normalMapFile, Filename colorMaskFile);
	}
}
