using Godot;

public partial class Asteroid : PoolableRB
{
    private float asteroidSpeed = 50f;

    [Export] private int initialhitsTillExplode = 1;

    private int hitsLeftTillExplode = 1;

    /// <summary>
    /// TODO: use a get GetHitComponent instead of detecting it in area individually on asteroids, enemy player for flash
    /// </summary>
    [Export] private Area2D area;

    [Export] private AnimationPlayer animationPlayer;

    [Export] private PixeliseShaderUpdateSize trailController;

    [Export] private AnimatedSprite2D sprite;

    // Direction the asteroid will move in (unit vector)
    private Vector2 moveDirection = Vector2.Right;

    [Export] AudioStreamPlayer2D hitByBulletSound;

    public override void _Ready()
    {
        // Connect signal for detecting collision
        area.AreaEntered += OnHit;

        // duplicate
        var flashMat = (ShaderMaterial)sprite.Material.Duplicate();
        sprite.Material = flashMat;
    }

    public void ToggleTrailOff()
    {
        trailController.TrailOff();
    }

    public void SetSpeed(double additionalSpeedIncrease)
    {
        trailController.TrailOn();

        // Pick a random direction to float in, normalize it
        moveDirection = new Vector2(GD.RandRange(-100, 100), GD.RandRange(-100, 100)).Normalized();

        // Rotate to face that direction
        GlobalRotation = moveDirection.Angle();

        // Set random speed
        double min = 30 + additionalSpeedIncrease;
        double max = 200 + additionalSpeedIncrease;
        asteroidSpeed = (float)GD.RandRange(min, max);

        // Initialize velocity to move asteroid in the chosen direction
        LinearVelocity = -moveDirection * asteroidSpeed;
    }

    /// <summary>
    /// Goddamit this on hit area flash and hits could be a separate component since this is the same for enemies and asteroids.
    /// </summary>
    /// <param name="area"></param>
    private void OnHit(Area2D area)
    {
        if (hitsLeftTillExplode > 1)
        {
            hitsLeftTillExplode--;
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

    public override void OnMadeVisibleAgain()
    {
        base.OnMadeVisibleAgain();
        hitsLeftTillExplode = initialhitsTillExplode;
        trailController.TrailOn();
    }
}
