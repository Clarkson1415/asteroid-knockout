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
    [Export] private bool ON = true;

    /// <summary>
    /// object scenes to choose from to spawn in.
    /// </summary>
    [Export] private Array<PackedScene> objectScenes;

    /// <summary>
    /// Number of asteroids within the screens between this and screensToCullAfter screen distance to stop spawning more at for performance.
    /// </summary>
    [Export] private int DensityThreshold = 300;

    /// <summary>
    /// TODO: NOT IMPLEMENTED YET PUT IN BASE OFFSCREEN SPAWNER CLASS.
    /// Number of screens away to start queue freeing asteroids for performance.
    /// </summary>
    private int screensToCullAfter;

    /// <summary>
    /// Invisible objects in the pool.
    /// </summary>
    private List<PoolableRB> objectPool = new();

    /// <summary>
    /// Visible objects in game and simulation.
    /// </summary>
    private List<PoolableRB> visibleObjects = new();

    /// <summary>
    /// The visible objects in the game and simulation.
    /// </summary>
    /// <returns></returns>
    public int GetVisibleAsteroids()
    {
        return visibleObjects.Count;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        if(!ON) { return; }

        CallDeferred(nameof(SpawnInitial));
    }

    private void Spawn()
    {
        var asteroidScene = objectScenes[GD.RandRange(0, objectScenes.Count - 1)];
        var obj = asteroidScene.Instantiate<PoolableRB>();
        GetTree().CurrentScene.AddChild(obj);
        obj.GlobalPosition = GetRandomPositionOffScreen();
        obj.OnDestroyed += (x) => AddToPool(x);
        AddToPool(obj);
    }

    private void SpawnInitial()
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

    /// <summary>
    /// Disable and turn off. put in pool to grab new ones from.
    /// </summary>
    /// <param name="obj"></param>
    private void AddToPool(PoolableRB obj)
    {
        OnPutInPool(obj);
        obj.DisableMode = CollisionObject2D.DisableModeEnum.Remove;
        obj.CallDeferred(Node.MethodName.SetProcessMode, (int)ProcessModeEnum.Disabled);
        obj.Visible = false;
        objectPool.Add(obj);
        visibleObjects.Remove(obj);
    }

    /// <summary>
    /// Something additioanl todo when obect is removed from simulation and put in obj pool.
    /// </summary>
    protected virtual void OnPutInPool(PoolableRB asteroid) { }

    private PoolableRB GetRandomAsteroidFromPool()
    {
        var obj = objectPool[GD.RandRange(0, objectPool.Count - 1)];
        objectPool.Remove(obj);
        obj.CallDeferred(Node.MethodName.SetProcessMode, (int)ProcessModeEnum.Inherit);
        obj.Visible = true;
        obj.OnMadeVisibleAgain();
        visibleObjects.Add(obj);
        return obj;
    }

    /// <summary>
    /// Get one from pool and set visible and put on screen.
    /// </summary>
    private void LoadAsteroidFromPool()
    {
        if (!ON) { return; }

        // Check visible is not over the count. Cull if there is too many.
        if (visibleObjects.Count > DensityThreshold)
        {
            // put the furthest asteroid visible away.
            var furthestAsteroid = visibleObjects.OrderByDescending(x => x.GlobalPosition.DistanceSquaredTo(this.GlobalPosition)).First();
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

    protected virtual void OnNewObjectSpawned(PoolableRB asteroid) { }

    private Vector2 GetRandomPositionOffScreen()
    {
        var viewport = GetViewport();
        var camera = viewport.GetCamera2D();
        var viewRect = new Rect2();
        viewRect.Position = GetViewportRect().Position;
        viewRect.Size = GetViewportRect().Size / camera.Zoom;

        // TODO: asteroids are being spawned inside view. :(
        Logger.Log($"view rect pos = {viewRect.Position}");
        Logger.Log($"view rect size = {viewRect.Size}");

        // random position within this area
        var random_offset = new Vector2((float)GD.RandRange(-viewRect.Size.X, viewRect.Size.X), (float)GD.RandRange(-viewRect.Size.Y, viewRect.Size.Y));

        // If within viewport move to closest point outside of the viewport. 
        // +32 for the asteroid size 1/2
        Vector2 spawnPoint = random_offset + GlobalPosition;

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
