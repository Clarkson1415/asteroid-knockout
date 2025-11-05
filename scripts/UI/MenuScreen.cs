using Godot;

/// <summary>
/// Represents a screen within a menu.
/// </summary>
[GlobalClass]
public partial class MenuScreen : Control
{
    [Export] public Button backButton;

    /// <summary>
    /// each screen has an initial focus button for controller support.
    /// </summary>
    [Export] private Button initialFocusButton;

    private bool inControllerMode;

    public override void _Ready()
    {
        Hide();
    }

    public override void _Input(InputEvent @event)
    {
        if (!this.Visible)
        {
            return;
        }

        if (@event is InputEventMouseMotion)
        {
            return;
        }

        // where ui_focus_next is the down left stick on a controller or wasd or arrow key.
        if (Input.IsActionJustPressed("ui_focus_next") && !inControllerMode)
        {
            inControllerMode = true;
            initialFocusButton.GrabFocus();
        }
    }

    public void Open()
    {
        this.Visible = true;
        inControllerMode = false;
    }

    public void Close()
    {
        this.Visible = false;
        inControllerMode = false;
    }
}
