using Microsoft.Xna.Framework;

namespace RenderBuddy
{
	public class DirectionLight
	{
		public Vector3 Direction { get; set; }

		public Color Color { get; set; }

		public DirectionLight(Vector3 direction, Color color)
		{
			Direction = direction;
			Direction.Normalize();
			Color = color;
		}
	}
}
