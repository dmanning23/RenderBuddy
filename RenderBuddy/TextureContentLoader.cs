using FilenameBuddy;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace RenderBuddy
{
    public class TextureContentLoader : ITextureLoader
    {
        public TextureInfo LoadImage(IRenderer renderer, Filename textureFile, Filename normalMapFile, Filename colorMaskFile)
        {
            var tex = new TextureInfo();

            if (null == textureFile || string.IsNullOrEmpty(textureFile.File))
            {
                throw new Exception("Empty textureFile in TextureContentLoader.LoadImage");
            }

            try
            {
                tex.Texture = renderer.Content.Load<Texture2D>(textureFile.GetRelPathFileNoExt());
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading textureFile in TextureContentLoader.LoadImage", ex);
            }

            try
            {
                if (null != normalMapFile && !string.IsNullOrEmpty(normalMapFile.File))
                {
                    tex.NormalMap = renderer.Content.Load<Texture2D>(normalMapFile.GetRelPathFileNoExt());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading normalMapFile in TextureContentLoader.LoadImage", ex);
            }

            try
            {
                if (null != colorMaskFile && !string.IsNullOrEmpty(colorMaskFile.File))
                {
                    tex.ColorMask = renderer.Content.Load<Texture2D>(colorMaskFile.GetRelPathFileNoExt());
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading colorMaskFile in TextureContentLoader.LoadImage", ex);
            }

            return tex;
        }
    }
}
