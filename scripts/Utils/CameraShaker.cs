using Godot;

/// <summary>
/// A camera shake class. add to scene as child of cam. camera shake is proportional to zoom so even shake at any zoom value.
/// </summary>
[GlobalClass]
public partial class CameraShaker : Node
{
	/// <summary>
	/// Value from 0 to 1. 0 being no camera shake. TODO: setup settings ui for this.
	/// </summary>
	public static float GLOBAL_CAMERA_SHAKE_INTENSITY;

    public static float DefaultDuration = 0.2f;
    public static float DefaultMagnitude = 2f;

    /// <summary>
    /// Start a shake using defaults.
    /// </summary>
    public static void MediumShake(Camera2D camera)
    {
        Shake(camera, DefaultDuration, DefaultMagnitude);
    }

    public static void LargeShake(Camera2D camera)
    {
        var largeShakeDuration = DefaultDuration * 1.2f;
        var largeShakeMag = DefaultMagnitude * 1.2f;
        Shake(camera, largeShakeDuration, largeShakeMag);
    }

    public static void SmallShake(Camera2D camera)
    {
        var smallShakeDuration = DefaultDuration * 0.8f;
        var smallShakeMag = DefaultMagnitude * 0.8f;
        Shake(camera, smallShakeDuration, smallShakeMag);
    }

    private static float currentShakeTimeLeft = 0f;
    private static float currentShakeDuration = 0f;
    private static float currentShakeMag = 0f;
    private static float Roughness = 1.0f; // how "jittery" each sample is

    private static Vector2 offsetbeforeShake = Vector2.Zero;
    private static Camera2D cameraToShake;
    private RandomNumberGenerator rand = new RandomNumberGenerator();
    private static bool currentlyShaking = false;

    public override void _Process(double delta)
    {
        if (cameraToShake == null) { return; }

        if (currentShakeTimeLeft <= 0 && currentlyShaking)
        {
            currentlyShaking = false;
            cameraToShake.Offset = offsetbeforeShake;
            return;
        }

        // decrease remaining time
        currentShakeTimeLeft -= (float)delta;

        float progress = 1f - (currentShakeTimeLeft / Mathf.Max(0.0001f, currentShakeDuration));

        // damping curve (smooth falloff)
        float damper = Mathf.SmoothStep(1f, 0f, progress);

        // generate a jitter vector scaled by roughness
        Vector2 jitter = new Vector2(
            rand.RandfRange(-1f, 1f),
            rand.RandfRange(-1f, 1f)
        );

        // make jitter direction length more varied
        if (jitter.Length() > 0.0001f)
            jitter = jitter.Normalized() * rand.RandfRange(0f, 1f);

        Vector2 offset = jitter * currentShakeMag * damper * Roughness;

        // Apply the shake offset relative to the base follow position
        // This line overrides the Position set in _PhysicsProcess with the offset.
        cameraToShake.Offset = offsetbeforeShake + offset;
    }

    /// <summary>
    /// Start a shake with explicit duration (seconds) and magnitude (pixels).
    /// </summary>
    private static void Shake(Camera2D camera, float duration, float intensity)
    {
        if (camera == null)
            return;

        cameraToShake = camera;
        offsetbeforeShake = cameraToShake.Offset;
        currentShakeTimeLeft = duration;
        currentShakeMag = intensity / camera.Zoom.X;
        currentlyShaking = true;
    }

    /// <summary>
    /// Add an impulse style shake on top of any existing shake (increases magnitude/time).
    /// </summary>
    //public void AddShake(Camera2D camera, float extraDuration, float extraMagnitude)
    //{
    //    if (_timeLeft <= 0f)
    //    {
    //        Shake(camera,extraDuration, extraMagnitude);
    //    }
    //    else
    //    {
    //        // extend/boost existing shake (simple additive model)
    //        _timeLeft = Mathf.Min(_totalDuration + extraDuration, _timeLeft + extraDuration);
    //        _totalDuration = Mathf.Max(_totalDuration, _timeLeft);
    //        _magnitude = Mathf.Max(_magnitude, extraMagnitude);
    //    }
    //}
}
