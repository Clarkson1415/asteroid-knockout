using cakegame1idk.scripts.GameObjects;
using Godot;
using System;

public abstract partial class PoolableRB : RigidBody2D, IPoolable
{
    /// <summary>
    /// Called when pulled from pool and readded to scene. e.g. reset hits and stuff.
    /// </summary>
    public virtual void OnMadeVisibleAgain()
    {
        ProcessMode = ProcessModeEnum.Inherit;
    }

    public event Action<IPoolable> OnDestroyed;

    public void InvokeDestroyedPoolableObject() => OnDestroyed?.Invoke(this);

    public virtual void OnAddedToPool()
    {
        DisableMode = CollisionObject2D.DisableModeEnum.Remove;
        ProcessMode = ProcessModeEnum.Disabled;
    }
}
