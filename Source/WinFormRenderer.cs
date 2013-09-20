using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using CameraBuddy;
using BasicPrimitiveBuddy;

namespace RenderBuddy
{
	public class WinFormRenderer : IRenderer<Image>
	{
		#region Members

		/// <summary>
		/// the form that this dude will render to
		/// </summary>
		public Form m_Form;

		#endregion

		#region Properties

		/// <summary>
		/// The camera we are going to use!
		/// </summary>
		public Camera Camera { get; private set; }

		/// <summary>
		/// thing for rendering primitives.
		/// </summary>
		/// <value>The primitive.</value>
		public IBasicPrimitive Primitive { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Hello, standard constructor!
		/// </summary>
		public WinFormRenderer(Form myForm)
		{
			Debug.Assert(null != myForm);
			m_Form = myForm;
		}

		public void Draw(Image image, Vector2 Position, Microsoft.Xna.Framework.Color rColor, float fRotation, bool bFlip, float fScale)
		{
			//Get teh grpahics object
			Graphics myGraphics = m_Form.CreateGraphics();

			//setup the scale matrix
			System.Drawing.Drawing2D.Matrix ScaleMatrix = new System.Drawing.Drawing2D.Matrix();
			ScaleMatrix.Scale(Camera.Scale, Camera.Scale);

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
				image = (Image)image.Clone();
				image.RotateFlip(RotateFlipType.RotateNoneFlipX);
			}

			myGraphics.Transform = TranslationMatrix;

			// Draw image
			myGraphics.DrawImage(image,
			                     Position.X,
			                     Position.Y,
			                     image.Width,
			                     image.Height);
		}

		public void Draw(Image image, Microsoft.Xna.Framework.Rectangle Destination,  Microsoft.Xna.Framework.Color rColor, float fRotation, bool bFlip)
		{
			//TODO: draw from a rectangle?
		}

		public Image LoadImage(string file)
		{
			if (File.Exists(file))
			{
				return null;
			}

			//Create the bitmap object, load from file
			Image myBitmap = Bitmap.FromFile(file);

			//TODO: embed a "flip" property?
			return myBitmap;
		}

		#endregion
	}
}