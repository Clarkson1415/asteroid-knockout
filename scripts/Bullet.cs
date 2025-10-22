using cakegame1idk.scripts;
using Godot;

public partial class Bullet : Node2D
{
	[Export] private float bulletSpeed = 800;

    [Export] private Area2D area;
    
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        area.AreaEntered += OnHit;
    }

    /// <summary>
    /// was shot by enemy make sure its not on the player bullet layer otherwise can kill itself.
    /// </summary>
    public void SetLayerToBeEnemysBullet()
    {
        // enemy bullet should not be on player bullet layer.
        area.SetCollisionLayerValue(GameLayers.PlayerBullets, false);

        area.SetCollisionLayerValue(GameLayers.EnemyBullets, true);

        // enemy bullet should collide with player ship.
        area.SetCollisionMaskValue(GameLayers.PlayerShip, true);

        // enemy bullet should not collide with enemy ship.
        area.SetCollisionMaskValue(GameLayers.EnemyShip, false);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        // move in the direction we are looking at.
        var velocity = new Vector2(0, -1).Rotated(GlobalRotation) * bulletSpeed;
        GlobalPosition += velocity * (float)delta;
    }

    private void OnHit(Area2D trigger)
    {
        this.QueueFree();
    }
}
