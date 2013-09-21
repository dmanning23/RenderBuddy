
namespace RenderBuddy
{
	/// <summary>
	/// This interface wraps up XNA.Texture2D or Winform.Image
	/// </summary>
	public interface ITexture
	{
		int Width { get; }
		int Height { get; }
	}
}