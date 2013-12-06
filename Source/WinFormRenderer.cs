using BasicPrimitiveBuddy;
using CameraBuddy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RenderBuddy
{
	public class WinFormRenderer : RendererBase
	{
		#region Members

		/// <summary>
		/// the form that this dude will render to
		/// </summary>
		public Form m_Form;

		private BufferedGraphicsContext m_CurrentContext;
		private BufferedGraphics m_DoubleBuffer;

		#endregion

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
		/// <param name="strBitmapFile">name of the bitmap to load</param>
		/// <returns>ID of the bitmap</returns>
		public override ITexture LoadImage(string file)
		{
			if (!File.Exists(file))
			{
				return null;
			}

			//Create the bitmap object, load from file
			WinFormTexture tex = new WinFormTexture()
			{
				Texture = Bitmap.FromFile(file)
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

		public override void Draw(ITexture image, Vector2 Position, Microsoft.Xna.Framework.Color rColor, float fRotation, bool bFlip, float fScale)
		{
			//get the winform texture out of there
			WinFormTexture tex = image as WinFormTexture;
			Image myImage = tex.Texture;

			//Get teh grpahics object
			Debug.Assert(null != m_DoubleBuffer);

			//setup the scale matrix
			System.Drawing.Drawing2D.Matrix ScaleMatrix = new System.Drawing.Drawing2D.Matrix();
			ScaleMatrix.Scale(fScale, fScale);

			//setup the rotation matrix
			System.Drawing.Drawing2D.Matrix RotationMatrix = new System.Drawing.Drawing2D.Matrix();
			RotationMatrix.Rotate(MathHelper.ToDegrees(fRotation));

			//setup the translation matrix
			System.Drawing.Drawing2D.Matrix TranslationMatrix = new System.Drawing.Drawing2D.Matrix();
			TranslationMatrix.Translate(Position.X, Position.Y);

			//setup the "move back from origin" matrix
			System.Drawing.Drawing2D.Matrix OriginMatrix = new System.Drawing.Drawing2D.Matrix();
			OriginMatrix.Translate(-Position.X, -Position.Y);

			TranslationMatrix.Multiply(RotationMatrix);
			TranslationMatrix.Multiply(ScaleMatrix);
			TranslationMatrix.Multiply(OriginMatrix);

			//take bFlip into account?
			if (bFlip)
			{
				//make a clone of the image and flip it... not very efficient, but this is tools anyways
				myImage = (Image)myImage.Clone();
				myImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
			}

			m_DoubleBuffer.Graphics.Transform = TranslationMatrix;

			// Draw image
			m_DoubleBuffer.Graphics.DrawImage(myImage,
			                     Position.X,
			                     Position.Y,
			                     myImage.Width,
			                     myImage.Height);
		}

		public override void Draw(ITexture image, Microsoft.Xna.Framework.Rectangle Destination, Microsoft.Xna.Framework.Color rColor, float fRotation, bool bFlip)
		{
			//TODO: draw from a rectangle?
		}

		#endregion
	}
}