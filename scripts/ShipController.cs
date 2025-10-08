using Godot;
using Godot.Collections;

public partial class ShipController : RigidBody2D
{
    [Export] public float MaxSpeed = 100f;
    [Export] private float boostSpeedMultiplier = 2f;

    [Export] public float Acceleration = 300f;
    [Export] public float Friction = 200f;
    [Export] public Node JoystickLeft;

    [Export] private Button fireButton;
    [Export] private Area2D area;

    // TODO: these will be able to be changed at runtime when picked up.
    [Export] private Engine currentEngine;
    [Export] private Weapon currentWeapon;
    private Vector2 inputVector;

    enum DAMAGE { none = 0, slight = 1, half = 2, completely = 3}

    private DAMAGE damageLevel = DAMAGE.none;

    [Export] Dictionary<DAMAGE, Texture2D> damageSprites;

    [Export] Sprite2D shipSprite;

    private bool isBoosting;

    [Export] private Button boostButton;

    public override void _Ready()
    {
        area.AreaEntered += OnHitAsteroid;
        CanSleep = false;
        shipSprite.Texture = damageSprites[damageLevel];
    }

    private void OnHitAsteroid(Area2D area)
    {
        // TODO: instead of isBoosting check for the speed of the collision.
        if (damageLevel == DAMAGE.completely || isBoosting)
        {
            Logger.Log("You died");
            return;
        }

        damageLevel += 1;
        shipSprite.Texture = damageSprites[damageLevel];
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        if (fireButton.ButtonPressed)
        {
            currentWeapon.Fire();
        }

        isBoosting = boostButton.ButtonPressed;

        Movement(dt);

        // Engine SFX
        if (isBoosting)
        {
            currentEngine.Boost();
        }
        else if (inputVector != Vector2.Zero)
        {
            currentEngine.PowerOn();
        }
        else
        {
            currentEngine.PowerOff();
        }
    }

    private void Movement(float dt)
    {
        inputVector = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        if (JoystickLeft != null && (bool)JoystickLeft.Get("is_pressed"))
        {
            inputVector = (Vector2)JoystickLeft.Get("output");
        }

        GlobalRotation = inputVector.Angle() + Mathf.Pi / 2;
        ApplyCentralForce(inputVector.Normalized() * Acceleration);

        // apply friction (dampen velocity manually)
        LinearVelocity = LinearVelocity.MoveToward(Vector2.Zero, Friction * dt);

        // Clamp max speed
        if (LinearVelocity.Length() > MaxSpeed)
        {
            var boost = isBoosting ? boostSpeedMultiplier : 1f;
            LinearVelocity = LinearVelocity.Normalized() * MaxSpeed * boost;
        }
    }
}
