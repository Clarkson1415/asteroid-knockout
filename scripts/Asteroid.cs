using Godot;
using System;

public partial class Asteroid : Node2D
{
	private float asteroidSpeed = 50;

	[Export] private Area2D area;

	[Export] private AnimationPlayer animationPlayer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		// pick random diretion to float in. Look At that direction.
		var randomDirection = new Vector2(GD.RandRange(-100, 100), GD.RandRange(-100, 100)).Normalized();
		GD.Print(randomDirection);
        GlobalRotation = randomDirection.Angle();
		asteroidSpeed = GD.RandRange(5, 30);
        area.AreaEntered += Explode;
    }

	private void Explode(Area2D area)
	{
        animationPlayer.Play("explode");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		// move in the direction we are looking at.
		var velocity = new Vector2(-1, 0).Rotated(GlobalRotation) * asteroidSpeed;
		GlobalPosition += velocity * (float)delta;
    }
}
