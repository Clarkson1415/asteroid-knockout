using Godot;

/// <summary>
/// Autoload.
/// </summary>
public partial class GameSettings : Node
{
	public bool TouchControlsOn = false;

	/// <summary>
	/// FPS and stuff.
	/// </summary>
	public bool debugOn = false;

    public static GameSettings Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }
}
