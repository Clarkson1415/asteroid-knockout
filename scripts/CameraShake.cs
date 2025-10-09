using Godot;
using System;

public partial class CameraShake : Camera2D
{
    // --- exported defaults you can tweak in the editor ---
    [Export] public float DefaultDuration = 0.5f;
    [Export] public float DefaultMagnitude = 16f; // pixels
    [Export] public float Roughness = 1.0f; // how "jittery" each sample is

    // --- internal state ---
    private Vector2 _originalPosition;
    private float _timeLeft = 0f;
    private float _totalDuration = 0f;
    private float _magnitude = 0f;
    private readonly RandomNumberGenerator _rng = new();

    public override void _Ready()
    {
        // store the camera's base position so shake offsets are additive and reversible
        _originalPosition = Position;
        _rng.Randomize();
    }

    public override void _Process(double delta)
    {
        if (_timeLeft > 0f)
        {
            // decrease remaining time
            _timeLeft -= (float)delta;

            // progress 0 -> 1 (0 at start, 1 at end)
            float progress = 1f - (_timeLeft / Math.Max(0.0001f, _totalDuration));

            // damping curve (smooth falloff). You can change to Mathf.Pow(1 - progress, 2) etc.
            float damper = Mathf.SmoothStep(1f, 0f, progress);

            // generate a jitter vector in [-1,1] for x and y, scaled by roughness
            Vector2 jitter = new Vector2(
                _rng.RandfRange(-1f, 1f),
                _rng.RandfRange(-1f, 1f)
            );

            // optional: make jitter direction length more varied
            if (jitter.Length() > 0.0001f)
                jitter = jitter.Normalized() * _rng.RandfRange(0f, 1f);

            // final offset = direction * magnitude * damper
            Vector2 offset = jitter * _magnitude * damper * Roughness;

            // apply relative to the stored original position
            Position = _originalPosition + offset;

            // when finished, ensure we snap back exactly to original to avoid drift
            if (_timeLeft <= 0f)
                Position = _originalPosition;
        }
        else
        {
            // make sure position remains exactly the original when not shaking
            if (Position != _originalPosition)
                Position = _originalPosition;
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
        _totalDuration = Math.Max(0.0001f, duration);
        _timeLeft = _totalDuration;
        _magnitude = Math.Max(0f, magnitude);
    }

    /// <summary>
    /// Add an impulse style shake on top of any existing shake (increases magnitude/time).
    /// Useful for stacking explosions/impacts.
    /// </summary>
    public void AddShake(float extraDuration, float extraMagnitude)
    {
        if (_timeLeft <= 0f)
        {
            // start fresh
            Shake(extraDuration, extraMagnitude);
        }
        else
        {
            // extend/boost existing shake (simple additive model)
            _timeLeft = Math.Min(_totalDuration + extraDuration, _timeLeft + extraDuration);
            _totalDuration = Math.Max(_totalDuration, _timeLeft);
            _magnitude = Math.Max(_magnitude, extraMagnitude);
        }
    }
}
