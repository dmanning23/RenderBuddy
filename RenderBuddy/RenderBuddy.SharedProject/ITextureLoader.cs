using FilenameBuddy;

namespace RenderBuddy
{
	public interface ITextureLoader
    {
		TextureInfo LoadImage(IRenderer renderer, Filename textureFile, Filename normalMapFile, Filename colorMaskFile);
	}
}
