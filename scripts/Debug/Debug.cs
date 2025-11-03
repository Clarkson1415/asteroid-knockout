using Godot;

public partial class Debug : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Visible = GameSettings.Instance.debugOn;
	}
}
