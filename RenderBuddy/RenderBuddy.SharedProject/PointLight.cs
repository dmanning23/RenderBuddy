using Microsoft.Xna.Framework;
using System;

namespace RenderBuddy
{
	public class PointLight
	{
		public Vector3 Position { get; set; }

		private float _brightness;
		public float Brightness
		{
			get { return _brightness; }
			set
			{
				_brightness = value;
			}
		}

		public Color Color { get; set; }

		public PointLight(Vector3 position, float brightness, Color color)
		{
			Position = position;
			Brightness = brightness;
			Color = color;
		}
	}
}
