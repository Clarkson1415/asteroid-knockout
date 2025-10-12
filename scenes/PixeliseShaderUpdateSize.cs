using Godot;
using System;

public partial class PixeliseShaderUpdateSize : CanvasGroup
{
    private ShaderMaterial material;
    private Camera2D camera;

    public override void _Ready()
    {
        material = this.Material as ShaderMaterial;
        camera = GetViewport().GetCamera2D();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        material.SetShaderParameter("camera_zoom", camera.Zoom);
	}
}
