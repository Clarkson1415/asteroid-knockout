using Godot;
using System;

/// <summary>
/// Button to bind controls.
/// </summary>
public partial class ControlsButton : Button
{
    [Export] private TextureRect controlMenu;

    public override void _Ready()
    {
        this.Pressed += ToggleControlMenu;
    }

    private void ToggleControlMenu()
    {
        controlMenu.Visible = !controlMenu.Visible;
    }
}
