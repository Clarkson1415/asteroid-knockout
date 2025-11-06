using Godot;

public partial class VsyncOnToggle : CheckButton
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.ButtonPressed = GameSettings.Instance.VsyncOn;
	}

    public override void _Toggled(bool toggledOn)
    {
        base._Toggled(toggledOn);

		GameSettings.Instance.SaveNewVsyncPreference(toggledOn);
    }
}
