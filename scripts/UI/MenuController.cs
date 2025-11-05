using Godot;
using Godot.Collections;
#nullable enable

/// <summary>
/// Controls some group of screens. e.g. manages a main menu or a pause scren etc.
/// </summary>
public partial class MenuController : Control
{
    protected MenuScreen? current = null;

    [Export] protected MenuScreen initial;

    [Export] protected Array<MenuScreen> screens;

    [Export] protected Button saveButton;

    [Export] protected Button quitButton;

    public override void _Ready()
    {
        base._Ready();

        saveButton.Pressed += () => Logger.Log("todo save");
        quitButton.Pressed += () => Logger.Log("todo quit");
    }
}
