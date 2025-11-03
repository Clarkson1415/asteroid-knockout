using Godot;

public partial class PauseMenu : TextureRect
{
    [Export] private Control initialFocusButton;

    private bool focused;

    public override void _Ready()
    {
        VisibilityChanged += Sync;
    }

    private void Sync()
    {
        Godot.Engine.TimeScale = this.Visible ? 0 : 1;
        focused = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
        {
            return;
        }

        if (Input.IsActionJustPressed("pause"))
        {
            this.Visible = !this.Visible;
            focused = false;
            Godot.Engine.TimeScale = this.Visible ? 0 : 1;
        }

        if (Input.IsActionJustPressed("ui_focus_next") && !focused)
        {
            focused = true;
            initialFocusButton.GrabFocus();
        }

        GlobalSignalBus.MenusOpen = this.Visible;
    }
}
