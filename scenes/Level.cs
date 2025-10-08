using Godot;
using System;

public partial class Level : Node
{
	/// <summary>
	/// every time this runs out level gets harder
	/// </summary>
	private float levelIncrementTime = 10f;

	[Export] private AstroidSpawner astroidSpawner;

	/// <summary>
	/// Level increments survived.
	/// </summary>
	private int levels = 0;

	public override void _Ready()
	{
		var timer = new Timer();
		AddChild(timer);
		timer.WaitTime = levelIncrementTime;
		timer.Timeout += IncreaseAsteroidSpeed;
	}

	private void IncreaseAsteroidSpeed()
	{
		astroidSpawner.IncreaseSpeeds(10f * (float)levels);
        levels++;
    }
}
