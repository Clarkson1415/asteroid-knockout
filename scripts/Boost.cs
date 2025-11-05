using cakegame1idk.scripts;
using cakegame1idk.scripts.GameObjects;
using Godot;
using System;

public partial class Boost : Area2D, IPoolable
{
    public event Action<IPoolable> OnDestroyed;

    [Export] private AnimationPlayer player;

    /// <summary>
    /// Managed by object pool.
    /// </summary>
    public void PrepareToRemoveFromScene()
    {
        player.Play("pickedup");
    }

    /// <summary>
    /// ONLY call from animation player
    /// </summary>
    public void CallFromAnimationPlayerToInvokeOnDestroyed()
    {
        OnDestroyed?.Invoke(this);
    }

    public void OnMadeVisibleAgain()
    {
        Monitorable = true;
        Monitoring = true;
        // Logger.Log("boost made visible again nothing implemented yet.");
    }

    public void OnAddedToPool()
    {
        Monitorable = false;
        Monitoring = false;
        // Logger.Log("boost OnAddedToPool nothing implemented yet.");
    }
}
