using Godot;
using Godot.Collections;
using System;

public partial class ShipController : RigidBody2D
{
    [Export] public float MaxSpeed = 100f;

    /// <summary>
    /// Multipler of how much bigger max speed is when boosting.
    /// </summary>
    [Export] private float boostMaxSpeedMultiplier = 2f;

    /// <summary>
    /// How fast boost accellerates.
    /// </summary>
    [Export] private float boostAccellerationMultiplier = 2f;

    [Export] public float Acceleration = 300f;
    [Export] public float Friction = 200f;
    [Export] public Node JoystickLeft;

    [Export] private TouchScreenButton fireButton;
    [Export] private TouchScreenButton boostButton;

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

    [Export] private AnimationPlayer shipSpriteAnimator;

    private string explode = "explode";

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
            if (shipSpriteAnimator.CurrentAnimation == explode || shipDestroyedHasBeenEmitted)
            {
                return;
            }

            shipSpriteAnimator.Play(explode);
            return;
        }

        damageLevel += 1;
        shipSprite.Texture = damageSprites[damageLevel];
    }

    private bool shipDestroyedHasBeenEmitted;

    /// <summary>
    /// TODO: to be called from animation player after animation of ship exploding
    /// </summary>
    public void OnShipDestroyed()
    {
        if (shipDestroyedHasBeenEmitted)
        {
            return;
        }

        shipDestroyedHasBeenEmitted = true;
        GlobalSignalBus.GetInstance().EmitShipDestroyed();
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        if (fireButton.IsPressed())
        {
            currentWeapon.Fire();
        }

        isBoosting = boostButton.IsPressed();

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

        // change dir only if input
        if (inputVector != Vector2.Zero)
        {
            GlobalRotation = inputVector.Angle() + Mathf.Pi / 2;
        }

        // apply force.
        var boost = isBoosting ? boostAccellerationMultiplier : 1f;

        // if no input but is boosing apply force in direction facing.
        var forceDirection = !isBoosting ? inputVector : new Vector2(Mathf.Sin(GlobalRotation), -Mathf.Cos(GlobalRotation));
        ApplyCentralForce(forceDirection.Normalized() * Acceleration * boost);

        // apply friction (dampen velocity manually)
        LinearVelocity = LinearVelocity.MoveToward(Vector2.Zero, Friction * dt);

        // Clamp max speed
        var maxSpeed = isBoosting ? (MaxSpeed * boostMaxSpeedMultiplier) : MaxSpeed;
        if (LinearVelocity.Length() > maxSpeed)
        {
            LinearVelocity = LinearVelocity.Normalized() * maxSpeed;
        }
    }
}
