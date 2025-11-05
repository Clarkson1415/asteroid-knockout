using Godot;

/// <summary>
/// Camera that follows target.
/// </summary>
public partial class FollowingCamera2D : Camera2D
{
    [Export] private ShipController followTarget;

    // Minimum allowed zoom (e.g., 1.0 is default, further out)a
    private const float MIN_ZOOM = 0.5f;

    // Maximum allowed zoom (e.g., 0.5 is closer/more zoomed in)
    private const float MAX_ZOOM = 0.8f;

    [Export] private float ZoomSmoothness = 0.01f;

    [Export(PropertyHint.Range, "0.0,20.0,0.1")] public float FollowSpeed = 15.0f;

    [Export] private ColorRect FogNode;

    [Export] public CameraShaker shaker;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _Process(double delta)
    {
        (FogNode.Material as ShaderMaterial).SetShaderParameter("pos", this.GlobalPosition);

        if (followTarget == null) return;
        Vector2 targetPos = followTarget.GlobalPosition;
        GlobalPosition = GlobalPosition.Lerp(targetPos, (float)delta * FollowSpeed);

        // camera zoom with speed:
        float speed = followTarget.LinearVelocity.Length();
        // scale speed to between max speed and speed = 0
        // then put camera zoom at that.
        float maxSpeed = followTarget.MaxSpeed;

        var speedOutOfMaxSpeed = speed / maxSpeed;

        // between min zoom and max zoom. min zoom = further out = faster so closer to max speed.
        // if speed = 0 -> target = MAX_ZOOM
        // if speed = max - > target zoom - Min_ZOOM
        // if speed = 0.5 then should be 0.75 which is between 0.5 and 1 of the max zoom min zoom.
        float targetZoom = Mathf.Lerp(MAX_ZOOM, MIN_ZOOM, speedOutOfMaxSpeed);
        Zoom = Zoom.Lerp(new Vector2(targetZoom, targetZoom), ZoomSmoothness);
    }
}