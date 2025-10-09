using Godot;
using System;

public partial class Asteroid : RigidBody2D
{
    private float asteroidSpeed = 50f;

    [Export] private int hitsTillExplode = 1;

    [Export] private Area2D area;
    [Export] private AnimationPlayer animationPlayer;

    [Export] private AnimatedSprite2D sprite;

    // Direction the asteroid will move in (unit vector)
    private Vector2 moveDirection = Vector2.Right;

    [Export] AudioStreamPlayer2D hitByBulletSound;

    public override void _Ready()
    {
        // Pick a random direction to float in, normalize it
        moveDirection = new Vector2(GD.RandRange(-100, 100), GD.RandRange(-100, 100)).Normalized();

        // Rotate to face that direction
        GlobalRotation = moveDirection.Angle();

        // Connect signal for detecting collision
        area.AreaEntered += OnHit;

        // duplicate
        var flashMat = (ShaderMaterial)sprite.Material.Duplicate();
        sprite.Material = flashMat;
    }

    public void SetSpeed(double additionalSpeedIncrease)
    {
        // Set random speed
        double min = 30 + additionalSpeedIncrease;
        double max = 200 + additionalSpeedIncrease;
        asteroidSpeed = (float)GD.RandRange(min, max);

        // Initialize velocity to move asteroid in the chosen direction
        LinearVelocity = -moveDirection * asteroidSpeed;
    }

    private void OnHit(Area2D area)
    {
        if (hitsTillExplode > 1)
        {
            hitsTillExplode--;
            hitByBulletSound.Play();

            // flash Red
            var spriteMaterial = (ShaderMaterial)sprite.Material;
            spriteMaterial.SetShaderParameter("flash_strength", 1f);

            var flashTimer = new Timer();
            flashTimer.OneShot = true;
            AddChild(flashTimer);
            flashTimer.WaitTime = 0.1f;
            flashTimer.Timeout += () => spriteMaterial.SetShaderParameter("flash_strength", 0f);
            flashTimer.Start();
            return;
        }

        if (animationPlayer.CurrentAnimation == "explode")
        {
            return;
        }

        animationPlayer.Play("explode");
        GlobalSignalBus.GetInstance().EmitOnAstroidDestroyedSignal();
    }

    //public override void _PhysicsProcess(double delta)
    //{
    //    // No manual position update needed: physics moves the asteroid based on LinearVelocity.

    //    // Optional: You could add drag/friction if you want to slow it down over time:
    //    //LinearVelocity = LinearVelocity.MoveToward(Vector2.Zero, drag * (float)delta);
    //}
}
