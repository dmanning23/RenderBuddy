using System;
using System.Collections.Generic;
using System.Text;
using GameTimer;

namespace RenderBuddy
{
	public interface ILight
	{
		/// <summary>
		/// If true, this light can be removed from the game
		/// </summary>
		bool IsDead { get; }

		void Update(GameClock clock);
	}
}
