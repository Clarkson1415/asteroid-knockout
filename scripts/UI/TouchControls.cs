using Godot;
using System;

/// <summary>
/// Control node that has children that are touch controls.
/// </summary>
public partial class TouchControls : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.Visible = GameSettings.Instance.TouchControlsOn;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
