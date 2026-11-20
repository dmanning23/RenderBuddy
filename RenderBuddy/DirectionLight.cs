using GameTimer;
using Microsoft.Xna.Framework;

namespace RenderBuddy
{
	public class DirectionLight : ILight
	{
		#region Properties

		public virtual bool IsDead
		{
			get
			{
				return false;
			}
		}

		public Vector3 Direction { get; set; }

		public Color Color { get; set; }

		#endregion //Properties

		#region Methods

		public DirectionLight(Vector3 direction, Color color)
		{
			Direction = direction;
			Direction.Normalize();
			Color = color;
		}

		public virtual void Update(GameClock clock)
		{
		}

		#endregion //Methods
	}
}
