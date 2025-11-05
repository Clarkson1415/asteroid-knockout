using Godot;

public partial class PixeliseShaderUpdateSize : CanvasGroup
{
    private ShaderMaterial material;
    private Camera2D camera;

    public override void _Ready()
    {
        this.Material = (ShaderMaterial)this.Material.Duplicate();
        material = (this.Material as ShaderMaterial);
        camera = GetViewport().GetCamera2D();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (material == null) { return; }
        material.SetShaderParameter("camera_zoom", camera.Zoom);
	}

    [Export] protected Node trailNode;

    public void TrailOn()
    {
        trailNode.Call("ToggleTrailOn");
    }

    public void TrailOff()
    {
        trailNode.Call("ToggleTrailOff");
    }
}
