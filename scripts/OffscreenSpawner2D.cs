using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Base class for all my offscreen spawners.
/// Uses an object pool.
/// EnemySpawner TODO Offsxcreen Spawner2d then inherit in asteroids and this
/// </summary>
public partial class OffscreenSpawner2D : Node2D
{
    [Export] private Array<PackedScene> asteroidScenes;

    /// <summary>
    /// TODO: PUT IN OFFSCREEN SPAWNER BASE CLASS. AND EDITBALE IN EDITOR.
    /// Number of asteroids within the screens between this and screensToCullAfter screen distance to stop spawning more at for performance.
    /// </summary>
    private int asteroidDensityThreshold = 300;

    /// <summary>
    /// TODO: NOT IMPLEMENTED YET PUT IN BASE OFFSCREEN SPAWNER CLASS.
    /// Number of screens away to start queue freeing asteroids for performance.
    /// </summary>
    private int screensToCullAfter;

    /// <summary>
    /// Invisible Asteroids.
    /// </summary>
    private List<Asteroid> objectPool = new();

    /// <summary>
    /// Visible asteroids.
    /// </summary>
    private List<Asteroid> visibleAsteroids = new();

    /// <summary>
    /// The visible objects in the game and simulation.
    /// </summary>
    /// <returns></returns>
    public int GetVisibleAsteroids()
    {
        return visibleAsteroids.Count;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        CallDeferred(nameof(SetupInitialAsteroids));
    }

    private void Spawn()
    {
        var asteroidScene = asteroidScenes[GD.RandRange(0, asteroidScenes.Count - 1)];
        var asteroid = asteroidScene.Instantiate<Asteroid>();
        GetTree().CurrentScene.AddChild(asteroid);
        asteroid.GlobalPosition = GetRandomPositionOffScreen();
        AddToPool(asteroid);
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

    private void AddToPool(Asteroid asteroid)
    {
        asteroid.ToggleTrailOff();
        asteroid.DisableMode = CollisionObject2D.DisableModeEnum.KeepActive;
        asteroid.Visible = false;
        objectPool.Add(asteroid);
        visibleAsteroids.Remove(asteroid);
    }

    private Asteroid GetRandomAsteroidFromPool()
    {
        var asteroid = objectPool[GD.RandRange(0, objectPool.Count - 1)];
        objectPool.Remove(asteroid);
        asteroid.DisableMode = CollisionObject2D.DisableModeEnum.Remove;
        asteroid.Visible = true;
        visibleAsteroids.Add(asteroid);
        return asteroid;
    }

    /// <summary>
    /// Get one from pool and set visible and put on screen.
    /// </summary>
    private void LoadAsteroidFromPool()
    {
        // Check visible is not over the count. Cull if there is too many.
        if (visibleAsteroids.Count >= asteroidDensityThreshold)
        {
            // put the furthest asteroid visible away.
            var furthestAsteroid = visibleAsteroids.OrderByDescending(x => x.GlobalPosition.DistanceSquaredTo(this.GlobalPosition)).First();
            AddToPool(furthestAsteroid);
        }
        else if (objectPool.Count == 0) // if we requested more asteroids than loaded, need to spawn more.
        {
            Spawn();
        }

        var asteroid = GetRandomAsteroidFromPool();
        asteroid.GlobalPosition = GetRandomPositionOffScreen();

        OnNewObjectSpawned(asteroid);
    }

    protected virtual void OnNewObjectSpawned(Asteroid asteroid) { }

    private Vector2 GetRandomPositionOffScreen()
    {
        var viewport = GetViewport();
        var viewRect = GetViewportRect();

        // random position within this area
        var random_offset = new Vector2((float)GD.RandRange(-viewRect.Size.X, viewRect.Size.X), (float)GD.RandRange(-viewRect.Size.Y, viewRect.Size.Y));

        // If within viewport move to closest point outside of the viewport. 
        // +32 for the asteroid size 1/2
        Vector2 spawnPoint = random_offset + GlobalPosition;

        var camera = viewport.GetCamera2D();
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
