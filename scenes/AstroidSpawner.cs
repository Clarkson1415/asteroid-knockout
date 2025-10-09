using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class AstroidSpawner : Area2D
{
	private RectangleShape2D collisionShape;

	[Export] private Array<PackedScene> asteroidScenes;

	private float additionalSpeedIncrease = 0f;

	public void IncreaseSpeeds(float speedsIncrease)
	{
		additionalSpeedIncrease += speedsIncrease;
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CollisionShape2D shape = this.GetChild<CollisionShape2D>(0);
		collisionShape = (RectangleShape2D)shape.Shape;

		var timer = new Timer();
		AddChild(timer);
		timer.WaitTime = 0.1f;
		timer.Start();
		timer.Timeout += Spawn;
		timer.Timeout += () => timer.Start();
		
		CallDeferred(nameof(Spawn10));
    }

	private void Spawn10()
	{
        // spawn 10 straight up.
        for (int i = 0; i < 20; i++)
        {
            Spawn();
        }
    }

	private void Spawn()
	{
        // random position within this control area
        var random_offset = new Vector2((float)GD.RandRange(-collisionShape.Size.X, collisionShape.Size.X), (float)GD.RandRange(-collisionShape.Size.Y, collisionShape.Size.Y));

        // If within viewport move to closest point outside of the viewport. 
        // +32 for the asteroid size 1/2
        Vector2 spawnPoint = random_offset + GlobalPosition;

        var camera = GetViewport().GetCamera2D();
        Vector2 topLeft = camera.GetScreenCenterPosition() - GetViewportRect().Size / 2;
        Rect2 worldViewport = new Rect2(topLeft, GetViewportRect().Size);

        if (worldViewport.HasPoint(spawnPoint))
		{
            // Add this so the whole asteroid is offscren.
            var asteroidSize = 32f;

            // get closest point on the edge of the viewport to random_offset
            // Determine which edge is closest
            float differenceToLeft = spawnPoint.X - worldViewport.Position.X;
            float differenceToRight = worldViewport.End.X - spawnPoint.X;
            float differenceToTop = spawnPoint.Y - worldViewport.Position.Y;
            float differenceToBottom = worldViewport.End.Y - spawnPoint.Y;

            float minDist = new List<float>() { differenceToLeft, differenceToRight, differenceToTop, differenceToBottom }.Min();

            if (minDist == differenceToLeft)
                spawnPoint.X = worldViewport.Position.X - asteroidSize;
            else if (minDist == differenceToRight)
                spawnPoint.X = worldViewport.End.X + asteroidSize;
            else if (minDist == differenceToTop)
                spawnPoint.Y = worldViewport.Position.Y - asteroidSize;
            else
                spawnPoint.Y = worldViewport.End.Y + asteroidSize;
        }

		var asteroidScene = asteroidScenes[GD.RandRange(0, asteroidScenes.Count - 1)];
		var asteroid = asteroidScene.Instantiate<Asteroid>();
		GetTree().CurrentScene.AddChild(asteroid);
		asteroid.GlobalPosition = spawnPoint;
		asteroid.SetSpeed(additionalSpeedIncrease);
    }
}
