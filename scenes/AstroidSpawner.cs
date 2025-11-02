using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class AstroidSpawner : OffscreenSpawner2D
{
	private float additionalSpeedIncrease = 0f;

    /// <summary>
    /// Debug label counting asteroids.
    /// </summary>
    [Export] Label countLabel;

    public void IncreaseSpeeds(float speedsIncrease)
	{
		additionalSpeedIncrease += speedsIncrease;
        Logger.Log($"Asteroid speed initial increased: {additionalSpeedIncrease}");
    }

    protected override void OnNewObjectSpawned(Asteroid asteroid)
    {
        asteroid.SetSpeed(additionalSpeedIncrease);
        countLabel.Text = $"Visible: {GetVisibleAsteroids()}.";
    }
}
