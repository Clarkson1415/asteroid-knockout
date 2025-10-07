using Godot;
using System;
using static Godot.TextServer;

public partial class ShipController : RigidBody2D
{
    [Export] public float MaxSpeed = 100f;
    [Export] public float Acceleration = 300f;
    [Export] public float Friction = 200f;
    [Export] public Node JoystickLeft;

    [Export] private Button fireButton;
    [Export] private Weapon currentWeapon;
    [Export] private Area2D area;

    public override void _Ready()
    {
        area.AreaEntered += OnHitAsteroid;

        Sleeping = false;
        CanSleep = false;
    }

    private void OnHitAsteroid(Area2D area)
    {
        GD.Print("Hit By Asteroid");
        // TODO: Damage the ship
    }

    public override void _PhysicsProcess(double delta)
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
        Vector2 inputVector = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

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
            LinearVelocity = LinearVelocity.Normalized() * MaxSpeed;
        }
    }
}
