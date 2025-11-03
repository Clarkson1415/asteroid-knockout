using Godot;

public partial class CameraShake : Camera2D
{
    [Export] private ShipController followTarget;

    // Minimum allowed zoom (e.g., 1.0 is default, further out)a
    private const float MIN_ZOOM = 0.5f;

    // Maximum allowed zoom (e.g., 0.5 is closer/more zoomed in)
    private const float MAX_ZOOM = 0.8f;

    [Export] private float ZoomSmoothness = 0.01f;

    [Export(PropertyHint.Range, "0.0,20.0,0.1")] public float FollowSpeed = 15.0f;

    // Shake Properties
    [Export] public float DefaultDuration = 0.5f;
    [Export] public float DefaultMagnitude = 16f; // pixels
    [Export] public float Roughness = 1.0f; // how "jittery" each sample is

    // Private State Variables
    private Vector2 _basePosition; // The position after following, but before shake
    private float _timeLeft = 0f;
    private float _totalDuration = 0f;
    private float _magnitude = 0f;
    private readonly RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        _rng.Randomize();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (followTarget == null) return;
        Vector2 targetPos = followTarget.GlobalPosition;
        Position = Position.Lerp(targetPos, (float)delta * FollowSpeed);
        _basePosition = Position;

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
    
    public override void _Process(double delta)
    {
        if (_timeLeft > 0f)
        {
            // decrease remaining time
            _timeLeft -= (float)delta;

            float progress = 1f - (_timeLeft / Mathf.Max(0.0001f, _totalDuration));

            // damping curve (smooth falloff)
            float damper = Mathf.SmoothStep(1f, 0f, progress);

            // generate a jitter vector scaled by roughness
            Vector2 jitter = new Vector2(
                _rng.RandfRange(-1f, 1f),
                _rng.RandfRange(-1f, 1f)
            );

            // make jitter direction length more varied
            if (jitter.Length() > 0.0001f)
                jitter = jitter.Normalized() * _rng.RandfRange(0f, 1f);

            Vector2 offset = jitter * _magnitude * damper * Roughness;

            // Apply the shake offset relative to the base follow position
            // This line overrides the Position set in _PhysicsProcess with the offset.
            Position = _basePosition + offset;

            if (_timeLeft <= 0f)
                Position = _basePosition;
        }
        else
        {
            // When not shaking, do nothing here. The _PhysicsProcess is 
            // continuously updating 'Position' for the follow effect.
        }
    }

    /// <summary>
    /// Start a shake using defaults.
    /// </summary>
    public void Shake()
    {
        Shake(DefaultDuration, DefaultMagnitude);
    }

    /// <summary>
    /// Start a shake with explicit duration (seconds) and magnitude (pixels).
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        // set up shake state (this will restart/override any current shake)
        _totalDuration = Mathf.Max(0.0001f, duration);
        _timeLeft = _totalDuration;
        _magnitude = Mathf.Max(0f, magnitude);
    }

    /// <summary>
    /// Add an impulse style shake on top of any existing shake (increases magnitude/time).
    /// </summary>
    public void AddShake(float extraDuration, float extraMagnitude)
    {
        if (_timeLeft <= 0f)
        {
            Shake(extraDuration, extraMagnitude);
        }
        else
        {
            // extend/boost existing shake (simple additive model)
            _timeLeft = Mathf.Min(_totalDuration + extraDuration, _timeLeft + extraDuration);
            _totalDuration = Mathf.Max(_totalDuration, _timeLeft);
            _magnitude = Mathf.Max(_magnitude, extraMagnitude);
        }
    }
}