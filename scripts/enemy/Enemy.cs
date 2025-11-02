using Godot;
using System.Linq;

public partial class Enemy : RigidBody2D
{
    [Export] NavigationAgent2D navigationAgent;
    private Node2D player;

    // Movement Properties
    [Export] private float accelleration = 100f;
    [Export] private float maxSpeed = 300f;

    // AI/Shooting Properties (Must be set in the Inspector)
    [Export] private float shootMinDistance = 15f; // the front of the sprites gun. TODO: although this doesnt work propertly. since just goes towards player if not in range.

    [Export] private float shootMaxDistance = 800f;
    [Export] private float shootMaxAngle = 30f; // Width of the firing cone in degrees
    [Export] private PackedScene bulletScene;
    [Export] private Node2D bulletStartPosition;

    [Export] private float fireRate = 1.0f; // Shots per second
    [Export] private float rotationSpeed = 0.5f;
    [Export] private Area2D getHit;
    [Export] private AnimatedSprite2D bodyAnimations;

    // State Variables
    private float timeSinceLastShot = 0f;
    private Vector2 targetDirection = Vector2.Zero; // Stores direction for drawing/shooting
    private Timer shootCooldown;
    private bool playerInRange = false;
    private bool facingPlayer = false;

    public override void _Ready()
    {
        // Find Player node using your defined method
        player = ShipController.Instance;
        navigationAgent.TargetPosition = player.GlobalPosition;

        // Enables _Process and _Draw calls
        SetProcess(true);

        // shoot
        shootCooldown = new Timer();
        this.AddChild(shootCooldown);
        shootCooldown.OneShot = true;
        shootCooldown.WaitTime = 1f / fireRate;

        // duplicate
        var flashMat = (ShaderMaterial)this.bodyAnimations.Material.Duplicate();
        this.bodyAnimations.Material = flashMat;

        getHit.AreaEntered += OnHit;
    }

    private void OnHit(Area2D area)
    {
        // can be hit by asteroids or bullets.
        this.bodyAnimations.Play("explode");
        this.bodyAnimations.AnimationFinished += () => this.QueueFree();
    }

    /// <summary>
    /// Handles the shooting timer and requests the debug visual to update.
    /// </summary>
    public override void _Process(double delta)
    {
        QueueRedraw();

        if (!shootCooldown.IsStopped() || !playerInRange)
        {
            return;
        }

        Shoot(targetDirection);
        shootCooldown.Start();
    }

    /// <summary>
    /// **TODO Solved:** Calculates movement/rotation and controls the shooting state.
    /// Directly manipulates the RigidBody2D's state for movement and rotation.
    /// </summary>
    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        if (player == null) return;

        var directionToPlayer = player.GlobalPosition - GlobalPosition;
        var distanceToPlayer = directionToPlayer.Length();
        var normalizedDirection = directionToPlayer.Normalized();
        targetDirection = normalizedDirection;

        Vector2 calculatedVelocity = Vector2.Zero;

        // --- 1. Range Check and Movement Control ---
        if (distanceToPlayer > shootMaxDistance)
        {
            // Outside range: Move using NavigationAgent
            navigationAgent.TargetPosition = player.GlobalPosition;
            calculatedVelocity = (navigationAgent.GetNextPathPosition() - GlobalPosition).Normalized() * accelleration;

            // Apply speed limit
            if (calculatedVelocity.Length() > maxSpeed)
            {
                calculatedVelocity = calculatedVelocity.Normalized() * maxSpeed;
            }

            playerInRange = false;
            Logger.Log("Out of range distnce: " + distanceToPlayer);
        }
        else
        {
            playerInRange = true;
        }

        float targetAngle = normalizedDirection.Angle() + (Mathf.Pi / 2);
        Logger.Log("target angle: " + targetAngle);

        AngularVelocity = Mathf.Sign(targetAngle - state.Transform.Rotation) * rotationSpeed;

        // Aiming check.
        var forwardVector = Vector2.Up.Rotated(this.Rotation);
        var angleToPlayer = forwardVector.AngleTo(normalizedDirection);
        facingPlayer = Mathf.Abs(Mathf.RadToDeg(angleToPlayer)) <= shootMaxAngle;

        if (facingPlayer)
        {
            AngularVelocity = 0; // Stop rotating when aimed.
        }

        Logger.Log($"is player in range: {playerInRange}.");
        Logger.Log($"fowards: {forwardVector}");
        Logger.Log($"angle to player: {angleToPlayer}.");

        // Apply the calculated velocity
        state.LinearVelocity = calculatedVelocity;
    }

    private void Shoot(Vector2 direction)
    {
        if (bulletScene != null)
        {
            var bullet = (Bullet)bulletScene.Instantiate();
            bullet.SetLayerToBeEnemysBullet();
            bullet.GlobalPosition = bulletStartPosition.GlobalPosition;
            bullet.Rotation = Rotation;
            // TODO: Add custom velocity/impulse logic here for the bullet
            GetParent().AddChild(bullet);
        }
    }

    /// <summary>
    /// Draws the debug shooting sector/cone on the screen using DrawPolygon.
    /// </summary>
    public override void _Draw()
    {
        float shootMaxAngleRad = Mathf.DegToRad(shootMaxAngle);

        float spriteFacingRotationOffset = -(Mathf.Pi/2);
        float startAngle = spriteFacingRotationOffset - shootMaxAngleRad;
        float endAngle = spriteFacingRotationOffset + shootMaxAngleRad;

        // Color logic remains the same
        Color rangeColor = (playerInRange && facingPlayer) ? new Color(0.2f, 1.0f, 0.2f, 0.2f) : new Color(1f, 0.2f, 0.2f, 0.2f);
        int pointCount = 32;

        var points = new Godot.Collections.Array<Vector2>();

        // 1. Add points along the outer arc (Max Distance)
        for (int i = 0; i <= pointCount; i++)
        {
            float angle = startAngle + (endAngle - startAngle) * (i / (float)pointCount);
            points.Add(Vector2.FromAngle(angle) * shootMaxDistance);
        }

        // 2. Add points along the inner arc (Min Distance), in reverse order
        for (int i = pointCount; i >= 0; i--)
        {
            float angle = startAngle + (endAngle - startAngle) * (i / (float)pointCount);
            points.Add(Vector2.FromAngle(angle) * shootMinDistance);
        }

        // Draw the filled polygon (the "Rect2" or sector)
        DrawPolygon(points.ToArray(), [rangeColor]);
    }
}