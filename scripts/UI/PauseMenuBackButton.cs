using Godot;

public partial class PauseMenuBackButton : Button
{
	[Export] private Control menuThisButtonCloses;

    public override void _Ready()
    {
        this.Pressed += () => menuThisButtonCloses.Visible = !menuThisButtonCloses.Visible;
    }
}
