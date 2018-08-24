using GameTimer;
using Microsoft.Xna.Framework;
using RandomExtensions;
using System;

namespace RenderBuddy
{
	/// <summary>
	/// This is a point light that has customizable attack, sustain, and delay, and a flare option
	/// </summary>
	public class FlarePointLight : PointLight
	{
		#region Properties

		private enum FlareState
		{
			Attack,
			Sustain,
			Delay,
			Dead,
		};

		private FlareState CurrentState { get; set; }

		private Random _rand;

		/// <summary>
		/// Clock that keeps track of the total TTL of this light
		/// </summary>
		private CountdownTimer TotalClock { get; set; }

		/// <summary>
		/// Clock used to time the different states
		/// </summary>
		private CountdownTimer StateClock { get; set; }

		/// <summary>
		/// Clock used to time when the flare brightness changes
		/// </summary>
		private CountdownTimer FlareClock { get; set; }

		/// <summary>
		/// amount of time between changing flare brightness
		/// </summary>
		public float FlareTimeDelta { get; set; }

		public float AttackTimeDelta { get; private set; }
		public float SustainTimeDelta { get; private set; }
		public float DelayTimeDelta { get; private set; }

		public float MinBrightness { get; set; }

		public float MaxBrightness { get; set; }

		public override bool IsDead
		{
			get
			{
				return CurrentState == FlareState.Dead || (!TotalClock.Paused && !TotalClock.HasTimeRemaining);
			}
		}

		public override float Brightness
		{
			get
			{
				switch (CurrentState)
				{
					case FlareState.Attack:
						{
							return base.Brightness * (1f - StateClock.Lerp);
						}
					case FlareState.Delay:
						{
							return base.Brightness * StateClock.Lerp;
						}
					case FlareState.Dead:
						{
							return 0f;
						}
					default:
						{
							return base.Brightness;
						}
				}
			}
			set
			{
				base.Brightness = value;
			}
		}

		/// <summary>
		/// A callback method to get the position of this emitter.
		/// null to figure out our own dang position
		/// </summary>
		private readonly Position3Delegate _positionDelegate;

		#endregion //Properties

		#region Methods

		public FlarePointLight(Vector3 position,
			Color color, 
			float flareTimeDelta,
			float attackTimeDelta,
			float sustainTimeDelta,
			float delayTimeDelta, 
			float minFlare, 
			float maxFlare,
			Position3Delegate positionDelegate = null) : base(position, 0f, color)
		{
			_positionDelegate = positionDelegate;
			if (null != _positionDelegate)
			{
				Position = _positionDelegate();
			}

			//setup the flare option
			_rand = new Random();
			MinBrightness = minFlare;
			MaxBrightness = maxFlare;
			FlareTimeDelta = flareTimeDelta;
			FlareClock = new CountdownTimer();
			FlareClock.Start(FlareTimeDelta);

			//setup the flash option
			CurrentState = FlareState.Attack;
			AttackTimeDelta = attackTimeDelta;
			SustainTimeDelta = sustainTimeDelta;
			DelayTimeDelta = delayTimeDelta;
			StateClock = new CountdownTimer();
			StateClock.Start(AttackTimeDelta);

			//Setup the total lifetime clock
			TotalClock = new CountdownTimer();
			if (SustainTimeDelta >= 0)
			{
				TotalClock.Start(AttackTimeDelta + SustainTimeDelta + DelayTimeDelta);
			}
			else
			{
				TotalClock.Paused = true;
			}

			UpdateFlareBrightness();
		}

		public void Kill()
		{
			TotalClock.Stop();
			CurrentState = FlareState.Dead;
		}

		public override void Update(GameClock clock)
		{
			base.Update(clock);
			TotalClock.Update(clock);
			StateClock.Update(clock);
			FlareClock.Update(clock);

			//update position from attached bone?
			if (null != _positionDelegate)
			{
				Position = _positionDelegate();
			}

			UpdateBrightness();
		}

		private void UpdateBrightness()
		{
			if (!FlareClock.HasTimeRemaining)
			{
				FlareClock.Start(FlareTimeDelta);
				UpdateFlareBrightness();
			}

			if (!StateClock.Paused && !StateClock.HasTimeRemaining)
			{
				switch (CurrentState)
				{
					case FlareState.Attack:
						{
							CurrentState = FlareState.Sustain;

							if (SustainTimeDelta >= 0)
							{
								StateClock.Start(SustainTimeDelta);
							}
							else
							{
								TotalClock.Paused = true;
								StateClock.Paused = true;
							}
						}
						break;
					case FlareState.Sustain:
						{
							CurrentState = FlareState.Delay;
							StateClock.Start(DelayTimeDelta);
						}
						break;
					case FlareState.Delay:
						{
							CurrentState = FlareState.Dead;
						}
						break;
				}
			}
		}

		private void UpdateFlareBrightness()
		{
			Brightness = _rand.NextFloat(MinBrightness, MaxBrightness);
		}

		#endregion //Methods
	}
}
