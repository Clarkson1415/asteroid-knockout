using Godot;

public partial class AstroidSpawner : Area2D
{
	private RectangleShape2D collisionShape;

	[Export] private PackedScene asteroidScene;

	private float additionalSpeedIncrease = 0f;

	public void IncreaseSpeeds(float speedsIncrease)
	{
		additionalSpeedIncrease += speedsIncrease;
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// random position within this control area
		CollisionShape2D shape = this.GetChild<CollisionShape2D>(0);
		collisionShape = (RectangleShape2D)shape.Shape;

		var timer = new Timer();
		AddChild(timer);
		timer.WaitTime = 1f;
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
        var random_offset = new Vector2((float)GD.RandRange(-collisionShape.Size.X, collisionShape.Size.X), (float)GD.RandRange(-collisionShape.Size.Y, collisionShape.Size.Y));

		var asteroid = asteroidScene.Instantiate<Asteroid>();
		GetTree().Root.AddChild(asteroid);
		asteroid.GlobalPosition = this.GlobalPosition + random_offset;
		asteroid.SetSpeed(additionalSpeedIncrease);
    }
}
