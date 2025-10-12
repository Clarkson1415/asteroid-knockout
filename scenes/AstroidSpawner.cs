using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class AstroidSpawner : Area2D
{
	private RectangleShape2D collisionShape;

	[Export] private Array<PackedScene> asteroidScenes;

	private float additionalSpeedIncrease = 0f;

    [Export] Label countLabel;

    /// <summary>
    /// Number of asteroids within the screens between this and screensToCullAfter to stop spawning more at.
    /// </summary>
    private int asteroidDensityThreshold = 300;

    /// <summary>
    /// Number of screens away to start queue freeing asteroids for performance.
    /// </summary>
    private int screensToCullAfter;

    /// <summary>
    /// Invisible Asteroids.
    /// </summary>
    private List<Asteroid> asteroidPool = new();

    /// <summary>
    /// Visible asteroids.
    /// </summary>
    private List<Asteroid> visibleAsteroids = new();

    public void IncreaseSpeeds(float speedsIncrease)
	{
		additionalSpeedIncrease += speedsIncrease;
        Logger.Log($"Asteroid speed initial increased: {additionalSpeedIncrease}");
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        CollisionShape2D shape = this.GetChild<CollisionShape2D>(0);
        collisionShape = (RectangleShape2D)shape.Shape;

        CallDeferred(nameof(SetupInitialAsteroids));
    }

    private void SetupInitialAsteroids()
	{
        for (int i = 0; i < 20; i++)
        {
            Spawn();
        }

        var timer = new Timer();
        AddChild(timer);
        timer.WaitTime = 0.3f;
        timer.Start();
        timer.Timeout += LoadAsteroidFromPool;
        timer.Timeout += () => timer.Start();
    }

	private void Spawn()
	{
		var asteroidScene = asteroidScenes[GD.RandRange(0, asteroidScenes.Count - 1)];
		var asteroid = asteroidScene.Instantiate<Asteroid>();
        GetTree().CurrentScene.AddChild(asteroid);
        asteroid.GlobalPosition = GetRandomPositionOffScreen();
        AddToPool(asteroid);
    }

    private void AddToPool(Asteroid asteroid)
    {
        asteroid.ToggleTrailOff();
        asteroid.DisableMode = DisableModeEnum.KeepActive;
        asteroid.Visible = false;
        asteroidPool.Add(asteroid);
        visibleAsteroids.Remove(asteroid);
    }

    private Asteroid GetRandomAsteroidFromPool()
    {
        var asteroid = asteroidPool[GD.RandRange(0, asteroidPool.Count - 1)];
        asteroidPool.Remove(asteroid);
        asteroid.DisableMode = DisableModeEnum.Remove;
        asteroid.Visible = true;
        visibleAsteroids.Add(asteroid);
        return asteroid;
    }

    /// <summary>
    /// Get one from pool and set visible and put on screen.
    /// </summary>
    private void LoadAsteroidFromPool()
    {
        // Check visible is not over the count and add visible to pool if it is.
        if (visibleAsteroids.Count >= asteroidDensityThreshold)
        {
            // put the furthest asteroid visible away.
            var furthestAsteroid = visibleAsteroids.OrderByDescending(x => x.GlobalPosition.DistanceSquaredTo(this.GlobalPosition)).First();
            AddToPool(furthestAsteroid);
        }
        else if (asteroidPool.Count == 0) // if we requested more asteroids. need to spawn more.
        {
            Spawn();
        }

        var asteroid = GetRandomAsteroidFromPool();
        asteroid.GlobalPosition = GetRandomPositionOffScreen();

        asteroid.SetSpeed(additionalSpeedIncrease);
        countLabel.Text = $"Visible: {visibleAsteroids.Count}.";
    }

    private Vector2 GetRandomPositionOffScreen()
    {
        // random position within this control area
        var random_offset = new Vector2((float)GD.RandRange(-collisionShape.Size.X, collisionShape.Size.X), (float)GD.RandRange(-collisionShape.Size.Y, collisionShape.Size.Y));

        // If within viewport move to closest point outside of the viewport. 
        // +32 for the asteroid size 1/2
        Vector2 spawnPoint = random_offset + GlobalPosition;

        var camera = GetViewport().GetCamera2D();
        Vector2 worldViewportSize = GetViewportRect().Size * camera.Zoom;
        Vector2 topLeft = camera.GetScreenCenterPosition() - worldViewportSize / 2;
        Rect2 worldViewport = new Rect2(topLeft, worldViewportSize);

        if (worldViewport.HasPoint(spawnPoint))
        {
            // Add this so the whole asteroid is offscren.
            var asteroidSize = 32f; // TODO: asteroid size varies a lot tho

            // get closest point on the edge of the viewport to random_offset
            // Determine which edge is closest
            float differenceToLeft = spawnPoint.X - worldViewport.Position.X;
            float differenceToRight = worldViewport.End.X - spawnPoint.X;
            float differenceToTop = spawnPoint.Y - worldViewport.Position.Y;
            float differenceToBottom = worldViewport.End.Y - spawnPoint.Y;

            float minDist = new List<float>() { differenceToLeft, differenceToRight, differenceToTop, differenceToBottom }.Min();

            if (minDist == differenceToLeft)
                spawnPoint.X = worldViewport.Position.X - asteroidSize;
            else if (minDist == differenceToRight)
                spawnPoint.X = worldViewport.End.X + asteroidSize;
            else if (minDist == differenceToTop)
                spawnPoint.Y = worldViewport.Position.Y - asteroidSize;
            else
                spawnPoint.Y = worldViewport.End.Y + asteroidSize;
        }

        return spawnPoint;
    }
}
