using Godot;

public abstract partial class PoolableRB : RigidBody2D
{
    [Signal] public delegate void OnDestroyedEventHandler(PoolableRB objDestroyed);

    public void EmitDestroyed()
    {
        EmitSignal(SignalName.OnDestroyed, this);
    }

    /// <summary>
    /// Called when pulled from pool and readded to scene. e.g. reset hits and stuff.
    /// </summary>
    public abstract void OnMadeVisibleAgain();
}
