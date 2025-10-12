using Godot;

public partial class PressAnyKeyToRestart : Control
{
    private bool active;

    public void ToggleOn()
    {
        active = true;
    }

    public override void _Input(InputEvent @event)
    {
        if (!active) { return; }

        if (@event is InputEventMouseMotion || @event is InputEventJoypadMotion)
        {
            return;
        }

        GetTree().ReloadCurrentScene();
    }
}
