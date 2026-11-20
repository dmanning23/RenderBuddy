using GameTimer;
using Microsoft.Xna.Framework;

namespace RenderBuddy
{
	/// <summary>
	/// This is a point light that flashs and quickly fades
	/// </summary>
	public class FlashPointLight : PointLight
	{
		#region Properties

		private CountdownTimer Clock { get; set; }

		public override bool IsDead
		{
			get
			{
				return !Clock.HasTimeRemaining;
			}
		}

		public override float Brightness
		{
			get
			{
				return base.Brightness * Clock.Lerp;
			}
			set
			{
				base.Brightness = value;
			}
		}

		#endregion //Properties

		#region Methods

		public FlashPointLight(Vector3 position, float brightness, Color color, float timeDelta) : base(position, brightness, color)
		{
			Clock = new CountdownTimer();
			Clock.Start(timeDelta);
		}

		public override void Update(GameClock clock)
		{
			base.Update(clock);
			Clock.Update(clock);
		}

		#endregion //Method
	}
}
