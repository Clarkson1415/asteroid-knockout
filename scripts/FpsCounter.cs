using Godot;

public partial class FpsCounter : Label
{
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		this.Text = Performance.GetMonitor(Performance.Monitor.TimeFps).ToString();
    }
}
