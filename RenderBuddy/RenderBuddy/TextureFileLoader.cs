using FilenameBuddy;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System;

namespace RenderBuddy
{
    public class TextureFileLoader : ITextureLoader
    {
        public TextureInfo LoadImage(IRenderer renderer, Filename textureFile, Filename normalMapFile, Filename colorMaskFile)
        {
            var tex = new TextureInfo();

            if (null == textureFile || string.IsNullOrEmpty(textureFile.File))
            {
                throw new Exception("Empty textureFile in TextureFileLoader.LoadImage");
            }

            try
            {
                using (var textureStream = File.OpenRead(textureFile.File))
                {
                    tex.Texture = Texture2D.FromStream(renderer.Graphics, textureStream);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading textureFile in TextureFileLoader.LoadImage", ex);
            }

            if (null != normalMapFile && !string.IsNullOrEmpty(normalMapFile.File))
            {
                try
                {
                    using (var normalStream = File.OpenRead(normalMapFile.File))
                    {
                        tex.NormalMap = Texture2D.FromStream(renderer.Graphics, normalStream);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error loading normalMapFile in TextureFileLoader.LoadImage", ex);
                }
            }

            if (null != colorMaskFile && !string.IsNullOrEmpty(colorMaskFile.File))
            {
                try
                {
                    using (var colorStream = File.OpenRead(colorMaskFile.File))
                    {
                        tex.ColorMask = Texture2D.FromStream(renderer.Graphics, colorStream);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error loading colorMaskFile in TextureFileLoader.LoadImage", ex);
                }
            }

            return tex;
        }
    }
}
