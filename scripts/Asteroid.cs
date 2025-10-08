using Godot;
using System;

public partial class Asteroid : RigidBody2D
{
    private float asteroidSpeed = 50f;

    [Export] private Area2D area;
    [Export] private AnimationPlayer animationPlayer;

    // Direction the asteroid will move in (unit vector)
    private Vector2 moveDirection = Vector2.Right;

    public override void _Ready()
    {
        // Pick a random direction to float in, normalize it
        moveDirection = new Vector2(GD.RandRange(-100, 100), GD.RandRange(-100, 100)).Normalized();

        // Rotate to face that direction
        GlobalRotation = moveDirection.Angle();

        // Connect signal for detecting collision
        area.AreaEntered += Explode;
    }

    public void SetSpeed(double additionalSpeedIncrease)
    {
        // Set random speed
        double min = 5 + additionalSpeedIncrease;
        double max = 30 + additionalSpeedIncrease;
        asteroidSpeed = (float)GD.RandRange(min, max);

        // Initialize velocity to move asteroid in the chosen direction
        LinearVelocity = -moveDirection * asteroidSpeed;
    }

    private void Explode(Area2D area)
    {
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
