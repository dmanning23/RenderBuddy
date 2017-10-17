using GameTimer;
using Microsoft.Xna.Framework;
using RandomExtensions;
using System;

namespace RenderBuddy
{
	/// <summary>
	/// This is a point light that flashs and quickly fades
	/// </summary>
	public class FlarePointLight : PointLight
	{
		#region Properties

		private Random _rand;

		private CountdownTimer Clock { get; set; }

		public float FlareTimeDelta { get; set; }

		public float MinBrightness { get; set; }

		public float MaxBrightness { get; set; }

		public override float Brightness
		{
			get
			{
				return base.Brightness;
			}
			set
			{
				base.Brightness = value;
			}
		}

		#endregion //Properties

		#region Methods

		public FlarePointLight(Vector3 position, float brightness, Color color, float flareTimeDelta, float min, float max) : base(position, brightness, color)
		{
			_rand = new Random();
			MinBrightness = min;
			MaxBrightness = max;
			FlareTimeDelta = flareTimeDelta;
			Clock = new CountdownTimer();
			Clock.Start(FlareTimeDelta);
		}

		public override void Update(GameClock clock)
		{
			base.Update(clock);
			Clock.Update(clock);

			if (!Clock.HasTimeRemaining)
			{
				Clock.Start(FlareTimeDelta);
				Brightness = _rand.NextFloat(MinBrightness, MaxBrightness);
			}
		}

		#endregion //Methods

	}
}
