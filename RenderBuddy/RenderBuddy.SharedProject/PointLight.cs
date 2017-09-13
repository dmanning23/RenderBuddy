using Microsoft.Xna.Framework;
using System;

namespace RenderBuddy
{
	public class PointLight
	{
		public Vector2 Position { get; set; }

		public float Radius { get; set; }

		private float _brightness;
		public float Brightness
		{
			get { return _brightness; }
			set
			{
				_brightness = Math.Min(1f, Math.Max(0f, value));
			}
		}

		public Color Color { get; set; }

		public PointLight(Vector2 position, float radius, float brightness, Color color)
		{
			Position = position;
			Radius = radius;
			Brightness = brightness;
			Color = color;
		}
	}
}
