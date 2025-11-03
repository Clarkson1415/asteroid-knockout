using Godot;

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

    protected override void OnNewObjectSpawned(PoolableRB obj)
    {
        if (obj is not Asteroid asteroid) { return; }
        asteroid.SetSpeed(additionalSpeedIncrease);
        countLabel.Text = $"Visible: {GetVisibleAsteroids()}.";
    }

    protected override void OnPutInPool(PoolableRB obj)
    {
        if (obj is not Asteroid asteroid) { return; }
        asteroid.ToggleTrailOff();
    }
}
