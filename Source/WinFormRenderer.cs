using BasicPrimitiveBuddy;
using FilenameBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RenderBuddy
{
	public class WinFormRenderer : RendererBase
	{
		#region Fields

		/// <summary>
		/// the form that this dude will render to
		/// </summary>
		public Form m_Form;

		private BufferedGraphicsContext m_CurrentContext;
		private BufferedGraphics m_DoubleBuffer;

		#endregion //Fields

		#region Methods

		/// <summary>
		/// Hello, standard constructor!
		/// </summary>
		public WinFormRenderer(Form myForm)
		{
			Debug.Assert(null != myForm);
			m_Form = myForm;
			m_CurrentContext = null;
			m_DoubleBuffer = null;

			Primitive = new WinFormBasicPrimitive();
		}

		public override void UnloadGraphicsContent()
		{
		}

		/// <summary>
		/// Load a bitmap into the system
		/// </summary>
		public override ITexture LoadImage(Filename textureFile, Filename normalMapFile = null, Filename colorMaskFile = null)
		{
			if (!File.Exists(textureFile.File))
			{
				return null;
			}

			//Create the bitmap object, load from file
			WinFormTexture tex = new WinFormTexture()
			{
				Texture = Bitmap.FromFile(textureFile.File)
			};

			//return the index it was stored at
			return tex;
		}

		public override void SpriteBatchBegin(BlendState myBlendState, Matrix translation)
		{
			// This example assumes the existence of a form called Form1.
			Debug.Assert(null == m_CurrentContext);
			Debug.Assert(null == m_DoubleBuffer);

			// Gets a reference to the current BufferedGraphicsContext
			m_CurrentContext = BufferedGraphicsManager.Current;

			// Creates a BufferedGraphics instance associated with Form1, and with 
			// dimensions the same size as the drawing surface of Form1.
			m_DoubleBuffer = m_CurrentContext.Allocate(m_Form.CreateGraphics(), m_Form.DisplayRectangle);

			//set the basicprimitive buddy
			var winFormPrimitive = Primitive as WinFormBasicPrimitive;
			Debug.Assert(null != winFormPrimitive);
			winFormPrimitive.DoubleBuffer = m_DoubleBuffer;
		}

		public override void SpriteBatchEnd()
		{
			Debug.Assert(null != m_CurrentContext);
			Debug.Assert(null != m_DoubleBuffer);

			//set the basicprimitive buddy
			var winFormPrimitive = Primitive as WinFormBasicPrimitive;
			Debug.Assert(null != winFormPrimitive);
			winFormPrimitive.DoubleBuffer = null;

			// This example assumes the existence of a BufferedGraphics instance called myBuffer.
			// Renders the contents of the buffer to the drawing surface associated with the buffer.
			m_DoubleBuffer.Render();

			// Renders the contents of the buffer to the specified drawing surface.
			m_DoubleBuffer.Render(m_Form.CreateGraphics());

			m_DoubleBuffer.Dispose();
			m_CurrentContext = null;
			m_DoubleBuffer = null;
		}

		public override void Draw(ITexture image, Vector2 position, Microsoft.Xna.Framework.Color primaryColor, Microsoft.Xna.Framework.Color secondaryColor, float rotation, bool isFlipped, float scale)
		{
			try
			{
				//get the winform texture out of there
				WinFormTexture tex = image as WinFormTexture;
				Image myImage = tex.Texture;

				//Get teh grpahics object
				Debug.Assert(null != m_DoubleBuffer);

				//setup the scale matrix
				System.Drawing.Drawing2D.Matrix scaleMatrix = new System.Drawing.Drawing2D.Matrix();
				scaleMatrix.Scale(scale, scale);

				//setup the rotation matrix
				System.Drawing.Drawing2D.Matrix rotationMatrix = new System.Drawing.Drawing2D.Matrix();
				rotationMatrix.Rotate(MathHelper.ToDegrees(rotation));

				//setup the translation matrix
				System.Drawing.Drawing2D.Matrix translationMatrix = new System.Drawing.Drawing2D.Matrix();
				translationMatrix.Translate(position.X, position.Y);

				//setup the "move back from origin" matrix
				System.Drawing.Drawing2D.Matrix originMatrix = new System.Drawing.Drawing2D.Matrix();
				originMatrix.Translate(-position.X, -position.Y);

				translationMatrix.Multiply(rotationMatrix);
				translationMatrix.Multiply(scaleMatrix);
				translationMatrix.Multiply(originMatrix);

				//take bFlip into account?
				if (isFlipped)
				{
					//make a clone of the image and flip it... not very efficient, but this is tools anyways
					myImage = (Image)myImage.Clone();
					myImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
				}

				m_DoubleBuffer.Graphics.Transform = translationMatrix;

				// Draw image
				m_DoubleBuffer.Graphics.DrawImage(myImage,
									 position.X,
									 position.Y,
									 myImage.Width,
									 myImage.Height);
			}
			catch (Exception ex)
			{
				//wtf do someinh
				MessageBox.Show(ex.ToString());
			}
		}

		public override void Draw(ITexture image, Microsoft.Xna.Framework.Rectangle destination, Microsoft.Xna.Framework.Color primaryColor, Microsoft.Xna.Framework.Color secondaryColor, float rotation, bool isFlipped)
		{
			//get the image size
			float scale = (float)destination.Width / (float)image.Width;

			//this gets screwed up casting from int to float
			if (scale <= 0.0f)
			{
				return;
			}

			//get a postion
			Vector2 pos = new Vector2(destination.X, destination.Y);
			Draw(image, pos, primaryColor, secondaryColor, rotation, isFlipped, scale);
		}

		#endregion //Methods
	}
}