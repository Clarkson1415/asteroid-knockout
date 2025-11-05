using cakegame1idk.scripts.GameObjects;
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
    private List<Node2D> objectPool = new();

    /// <summary>
    /// Visible objects in game and simulation.
    /// </summary>
    private List<Node2D> visibleObjects = new();

    private Timer timer;

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
        if(!ON) 
        { 
            return; 
        }

        CallDeferred(nameof(SpawnInitial));
    }

    private void Spawn()
    {
        var asteroidScene = objectScenes[GD.RandRange(0, objectScenes.Count - 1)];
        Node2D obj = asteroidScene.Instantiate<Node2D>();
        GetTree().CurrentScene.AddChild(obj);
        var poolableObj = ReturnIPoolable(obj);
        poolableObj.OnDestroyed += (obj) => AddToPool(obj as Node2D);
        AddToPool(obj);
    }

    private IPoolable ReturnIPoolable(Node2D node)
    {
        var poolableObj = node as IPoolable; 

        if (poolableObj == null)
        {
            Logger.LogError($"object trying to spawn is not poolable! implement IPoolable interface. on spawner: {this.Name}");
        }

        return poolableObj;
    }

    [Export] float minInterval = 2f;
    [Export] float maxInterval = 10f;

    private RandomNumberGenerator random;

    [Export] private int objectPoolInitialSize;

    [Export] private int initialOnSpawn;

    private void SpawnInitial()
    {
        // cache an amount
        for (int i = 0; i < objectPoolInitialSize; i++)
        {
            Spawn();
        }

        // spawn an initail amount
        for (int i = 0; i < initialOnSpawn; i++)
        {
            LoadFromPool();
        }

        random = new RandomNumberGenerator();
        timer = new Timer();
        AddChild(timer);
        timer.OneShot = true;
        timer.Timeout += RestartTimer;
        timer.Start();
    }

    private void RestartTimer()
    {
        LoadFromPool();
        var interval = random.RandfRange(minInterval, maxInterval);
        timer.WaitTime = interval;
        timer.Start();
    }

    /// <summary>
    /// Disable and turn off. put in pool to grab new ones from.
    /// </summary>
    /// <param name="obj"></param>
    private void AddToPool(Node2D obj)
    {
        var poolableObj = ReturnIPoolable(obj);
        poolableObj.OnAddedToPool();
        obj.SetVisible(false);
        objectPool.Add(obj);
        visibleObjects.Remove(obj);
    }

    private Node2D GetRandomFromPool()
    {
        var obj = objectPool[GD.RandRange(0, objectPool.Count - 1)];
        objectPool.Remove(obj);
        obj.SetVisible(true);
        obj.SetGlobalPosition(GetRandomPositionOffScreen());
        var poolableObj = ReturnIPoolable(obj);
        poolableObj.OnMadeVisibleAgain();
        visibleObjects.Add(obj);
        return obj;
    }

    private bool IsOffscreen(Node2D obj)
    {
        var camera = GetViewport().GetCamera2D();
        if (camera == null) return false;

        Vector2 viewportSize = GetViewportRect().Size / camera.Zoom;
        Vector2 topLeft = camera.GetScreenCenterPosition() - viewportSize / 2f;
        Rect2 worldViewport = new Rect2(topLeft, viewportSize);

        return !worldViewport.HasPoint(obj.GlobalPosition);
    }

    /// <summary>
    /// Get one from pool and set visible and put on screen.
    /// </summary>
    private void LoadFromPool()
    {
        if (!ON) { return; }

        // Check visible is not over the count. Cull if there is too many.
        if (visibleObjects.Count > DensityThreshold)
        {
            // put the furthest asteroid visible away.
            var furthestAsteroid = visibleObjects.OrderByDescending(x => x.GlobalPosition.DistanceSquaredTo(this.GlobalPosition)).First();
            if (!IsOffscreen(furthestAsteroid))
            {
                return;
            }

            AddToPool(furthestAsteroid);
        }
        else if (objectPool.Count == 0) // if we requested more asteroids than loaded, need to spawn more.
        {
            Spawn();
        }

        var asteroid = GetRandomFromPool();
        asteroid.GlobalPosition = GetRandomPositionOffScreen();

        OnNewObjectSpawned(asteroid);
    }

    /// <summary>
    /// When object taken from pool and made or re made visible.
    /// </summary>
    /// <param name="asteroid"></param>
    protected virtual void OnNewObjectSpawned(Node2D asteroid) { }

    private Vector2 GetRandomPositionOffScreen()
    {
        var camera = GetViewport().GetCamera2D();
        if (camera == null)
            return Vector2.Zero;

        Vector2 viewportSize = GetViewportRect().Size / camera.Zoom;
        Vector2 topLeft = camera.GetScreenCenterPosition() - viewportSize / 2f;
        Vector2 bottomRight = topLeft + viewportSize;

        float margin = 64f; // buffer so it’s properly offscreen
        Vector2 spawnPos = Vector2.Zero;

        int side = GD.RandRange(0, 3); // 0=top,1=bottom,2=left,3=right
        switch (side)
        {
            case 0: // Top
                spawnPos = new Vector2(
                    (float)GD.RandRange(topLeft.X - margin, bottomRight.X + margin),
                    topLeft.Y - margin
                );
                break;
            case 1: // Bottom
                spawnPos = new Vector2(
                    (float)GD.RandRange(topLeft.X - margin, bottomRight.X + margin),
                    bottomRight.Y + margin
                );
                break;
            case 2: // Left
                spawnPos = new Vector2(
                    topLeft.X - margin,
                    (float)GD.RandRange(topLeft.Y - margin, bottomRight.Y + margin)
                );
                break;
            case 3: // Right
                spawnPos = new Vector2(
                    bottomRight.X + margin,
                    (float)GD.RandRange(topLeft.Y - margin, bottomRight.Y + margin)
                );
                break;
        }

        return spawnPos;
    }
}
