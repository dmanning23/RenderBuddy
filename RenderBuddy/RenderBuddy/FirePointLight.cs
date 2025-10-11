using GameTimer;
using Microsoft.Xna.Framework;
using RandomExtensions;
using System;

namespace RenderBuddy
{
	/// <summary>
	/// This is a point light that sparkles like a firelight
	/// </summary>
	public class FirePointLight : PointLight
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

		public FirePointLight(Vector3 position, Color color, float flareTimeDelta, float min, float max) : base(position, 0f, color)
		{
			_rand = new Random();
			MinBrightness = min;
			MaxBrightness = max;
			FlareTimeDelta = flareTimeDelta;
			Clock = new CountdownTimer();
			Clock.Start(FlareTimeDelta);
			UpdateFlareBrightness();
		}

		public override void Update(GameClock clock)
		{
			base.Update(clock);
			Clock.Update(clock);

			if (!Clock.HasTimeRemaining)
			{
				Clock.Start(FlareTimeDelta);
				UpdateFlareBrightness();
			}
		}

		private void UpdateFlareBrightness()
		{
			Brightness = _rand.NextFloat(MinBrightness, MaxBrightness);
		}

		#endregion //Methods

	}
}
