using Godot;
using System;

public partial class RestartButton : TouchScreenButton
{
    public override void _Input(InputEvent @event)
    {
        if (this.IsPressed())
        {
            // TODO some animation transition stuff.
            Logger.Log("RELOADING TODO some animation transition stuff.");
            GetTree().ReloadCurrentScene();
        }
    }
}
