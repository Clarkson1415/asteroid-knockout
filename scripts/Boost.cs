using cakegame1idk.scripts.GameObjects;
using Godot;
using System;

public partial class Boost : Area2D, IPoolable
{
    public event Action<IPoolable> OnDestroyed;

    public void InvokeDestroyedPoolableObject()
    {
        OnDestroyed?.Invoke(this);
    }

    public void OnMadeVisibleAgain()
    {
        Logger.Log("boost made visible again nothing implemented yet.");
    }

    public void OnAddedToPool()
    {
        Logger.Log("boost OnAddedToPool nothing implemented yet.");
    }
}
