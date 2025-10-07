using Godot;

public partial class ShipController : Node2D
{
    [Export] public float MaxSpeed = 100f;
    [Export] public float Acceleration = 300f;
    [Export] public float Friction = 200f;
    [Export] public Node JoystickLeft; // Reference to GDScript VirtualJoystick

    private Vector2 _velocity = Vector2.Zero;

    [Export] private Button fireButton;

    /// <summary>
    /// TODO: loaded at runtime later.
    /// </summary>
    [Export] private Weapon currentWeapon;

    [Export] private Area2D area;

    public override void _Ready()
    {
        area.AreaEntered += OnHitAsteroid;
    }

    private void OnHitAsteroid(Area2D area)
    {
        GD.Print("Hit By Asteroid");
        // TODO change health. by changing the ship sprite to the damaged.

    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (fireButton.ButtonPressed)
        {
            currentWeapon.Fire();
        }

        Movement(dt);
    }

    private void Movement(float dt)
    {
        // Get keyboard input vector
        Vector2 inputVector = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        // Use joystick if it's active
        if (JoystickLeft != null && (bool)JoystickLeft.Get("is_pressed"))
        {
            inputVector = (Vector2)JoystickLeft.Get("output");
        }

        if (inputVector != Vector2.Zero)
        {
            // Rotate to face direction
            GlobalRotation = inputVector.Angle();

            // Accelerate toward desired velocity
            Vector2 desiredVelocity = inputVector * MaxSpeed;
            _velocity = _velocity.MoveToward(desiredVelocity, Acceleration * dt);
        }
        else
        {
            // Apply friction
            _velocity = _velocity.MoveToward(Vector2.Zero, Friction * dt);
        }

        // Move the object
        Position += _velocity * dt;
    }
}
