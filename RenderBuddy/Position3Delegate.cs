using Microsoft.Xna.Framework;

namespace RenderBuddy
{
	/// <summary>
	/// This is a callback method for getting a position
	/// used to break out dependencies
	/// </summary>
	/// <returns>a method to get a position.</returns>
	public delegate Vector3 Position3Delegate();
}
