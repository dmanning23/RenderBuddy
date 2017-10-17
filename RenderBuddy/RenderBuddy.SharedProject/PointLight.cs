using GameTimer;
using Microsoft.Xna.Framework;

namespace RenderBuddy
{
	public class PointLight : ILight
	{
		#region Properties

		public virtual bool IsDead
		{
			get
			{
				return false;
			}
		}

		public Vector3 Position { get; set; }

		private float _brightness;
		public virtual float Brightness
		{
			get { return _brightness; }
			set
			{
				_brightness = value;
			}
		}

		public Color Color { get; set; }

		#endregion //Properties

		#region Methods

		public PointLight(Vector3 position, float brightness, Color color)
		{
			Position = position;
			Brightness = brightness;
			Color = color;
		}

		public virtual void Update(GameClock clock)
		{
		}

		#endregion //Methods
	}
}
