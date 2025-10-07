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

        // Set random speed
        asteroidSpeed = GD.RandRange(5, 30);

        // Connect signal for detecting collision
        area.AreaEntered += Explode;

        // Initialize velocity to move asteroid in the chosen direction
        LinearVelocity = -moveDirection * asteroidSpeed;
    }

    private void Explode(Area2D area)
    {
        animationPlayer.Play("explode");
    }

    public override void _PhysicsProcess(double delta)
    {
        // No manual position update needed: physics moves the asteroid based on LinearVelocity.

        // Optional: You could add drag/friction if you want to slow it down over time:
        //LinearVelocity = LinearVelocity.MoveToward(Vector2.Zero, drag * (float)delta);
    }
}
