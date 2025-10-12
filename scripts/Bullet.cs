using Godot;

public partial class Bullet : Node2D
{
	[Export] private float bulletSpeed = 800;

    [Export] private Area2D area;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        area.AreaEntered += OnHitAsteroid;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        // move in the direction we are looking at.
        var velocity = new Vector2(0, -1).Rotated(GlobalRotation) * bulletSpeed;
        GlobalPosition += velocity * (float)delta;
    }

    private void OnHitAsteroid(Area2D area)
    {
        this.QueueFree();
    }
}
