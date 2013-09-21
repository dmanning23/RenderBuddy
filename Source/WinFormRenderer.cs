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
	public class WinFormRenderer : IRenderer
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

		public void Draw(ITexture image, Vector2 Position, Microsoft.Xna.Framework.Color rColor, float fRotation, bool bFlip, float fScale)
		{
			//get the winform texture out of there
			WinFormTexture tex = image as WinFormTexture;
			Image myImage = tex.Texture;

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
				myImage = (Image)myImage.Clone();
				myImage.RotateFlip(RotateFlipType.RotateNoneFlipX);
			}

			myGraphics.Transform = TranslationMatrix;

			// Draw image
			myGraphics.DrawImage(myImage,
			                     Position.X,
			                     Position.Y,
			                     myImage.Width,
			                     myImage.Height);
		}

		public void Draw(ITexture image, Microsoft.Xna.Framework.Rectangle Destination,  Microsoft.Xna.Framework.Color rColor, float fRotation, bool bFlip)
		{
			//TODO: draw from a rectangle?
		}

		public ITexture LoadImage(string file)
		{
			if (File.Exists(file))
			{
				return null;
			}

			//Create the bitmap object, load from file
			WinFormTexture tex = new WinFormTexture()
			{
				Texture = Bitmap.FromFile(file)
			};

			//TODO: embed a "flip" property?
			return tex;
		}

		#endregion
	}
}