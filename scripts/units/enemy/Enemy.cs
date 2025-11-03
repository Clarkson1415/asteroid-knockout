using Godot;
using System.Linq;

public partial class Enemy : PoolableRB
{
    //[Export] NavigationAgent2D navigationAgent;

    private ShipController player;

    // Movement Properties
    [Export] private float accelleration = 100f;
    [Export] private float maxSpeed = 300f;

    // the front of the sprites gun. TODO: although this doesnt work propertly. since just goes towards player if not in range.
    [Export] private float shootMinDistance = 15f; 
    [Export] private float shootMaxDistance = 100f;
    [Export] private float shootMaxAngle = 1f; // Width of the firing cone in degrees
    [Export] private PackedScene bulletScene;
    [Export] private Node2D bulletStartPosition;

    [Export] private float fireRate = 1.0f; // Shots per second
    [Export] private float rotationSpeed = 100f;
    [Export] private Area2D getHit;
    [Export] private AnimatedSprite2D bodyAnimations;

    // State Variables
    private float timeSinceLastShot = 0f;
    private Timer shootCooldown;
    private bool playerInRange = false;
    private bool playerDirectlyinFront = false;

    public override void _Ready()
    {
        // Find Player node using your defined method
        player = ShipController.Instance;

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

    /// <summary>
    /// TODO go in get hit compopnent.
    /// </summary>
    [Export] private AudioStreamPlayer2D gotHitSound;

    [Export] private AudioStreamPlayer2D explodeSound;

    [Export] private int initialHitsTillDestroyed = 2;

    private int HitsLeft = 2;

    private void OnHit(Area2D area)
    {
        if (HitsLeft < 1) 
        { 
            return; 
        }

        // can be hit by asteroids or bullets.

        // TODO; PUT HITS, SOUNDS EXPLODE and hit.
        if (HitsLeft > 1)
        {
            HitsLeft--;
            // TODO: PUT animation flash in hit component. in asteroids and player too.
            var spriteMaterial = (ShaderMaterial)this.bodyAnimations.Material;
            spriteMaterial.SetShaderParameter("flash_strength", 1f);
            var flashTimer = new Timer();
            flashTimer.OneShot = true;
            AddChild(flashTimer);
            flashTimer.WaitTime = 0.1f;
            flashTimer.Timeout += () => spriteMaterial.SetShaderParameter("flash_strength", 0f);
            flashTimer.Start();

            // TODO: move this to get hit component.
            gotHitSound.Play();
            return;
        }

        explodeSound.Play();
        this.bodyAnimations.Play("explode");
        this.bodyAnimations.AnimationFinished += EmitDestroyed;

        // Does not get disposed it goes back into the object pooler.
        // Emit destroyed is emitteed in animation player.
    }

    /// <summary>
    /// Handles the shooting timer and requests the debug visual to update.
    /// </summary>
    public override void _Process(double delta)
    {
        if (!Visible) { return; }

        if (player.IsDead)
        {
            return;
        }

        if (HitsLeft < 1) // Dead
        {
            return;
        }

        QueueRedraw();

        if (!shootCooldown.IsStopped() || !playerInRange)
        {
            return;
        }

        var forwardVector = Vector2.Up.Rotated(this.Rotation);
        Shoot(forwardVector);
        shootCooldown.Start();
    }

    /// <summary>
    /// Directly manipulates the RigidBody2D's state for movement and rotation.
    /// </summary>
    public override void _PhysicsProcess(double delta)
    {
        if (HitsLeft < 1)
        {
            return;
        }

        if (!Visible) { return; }

        if (player == null) return;

        var dt = (float)delta;

        var normDirectionToTargetPos = GlobalPosition.DirectionTo(player.GlobalPosition);
        var distanceToPlayer = this.GlobalPosition.DistanceTo(player.GlobalPosition);

        // Angle.
        float targetAngle = normDirectionToTargetPos.Angle() + Mathf.Pi / 2;

        // Compute shortest angle difference (-π..π)
        float angleDiff = Mathf.Wrap(targetAngle - Rotation, -Mathf.Pi, Mathf.Pi);

        playerDirectlyinFront = Mathf.Abs(angleDiff) < Mathf.DegToRad(shootMaxAngle);

        if (!playerDirectlyinFront)
        {
            float rotationStep = rotationSpeed * dt;
            AngularVelocity = Mathf.Clamp(angleDiff / dt, -rotationStep, rotationStep);
        }
        else
        {
            AngularVelocity = 0;
        }

        playerInRange = playerDirectlyinFront && (distanceToPlayer < shootMaxDistance);

        // if in range OR the player is NOT infront. then should slow down.
        if (playerInRange || !playerDirectlyinFront)
        {
            // apply friction (dampen velocity manually)
            LinearVelocity = LinearVelocity.MoveToward(Vector2.Zero, accelleration * dt);
        }
        else // its outside range AND player is directly in front
        {
            var forwardVector = Vector2.Up.Rotated(this.Rotation);
            ApplyCentralForce(forwardVector.Normalized() * this.accelleration);

            if (LinearVelocity.Length() > maxSpeed)
            {
                LinearVelocity = LinearVelocity.Normalized() * maxSpeed;
            }
        }
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
        return;

        if (HitsLeft < 1)
        {
            return;
        }

        if (!Visible) { return; }

        float shootMaxAngleRad = Mathf.DegToRad(shootMaxAngle);

        float spriteFacingRotationOffset = -(Mathf.Pi/2);
        float startAngle = spriteFacingRotationOffset - shootMaxAngleRad;
        float endAngle = spriteFacingRotationOffset + shootMaxAngleRad;

        // Color logic remains the same
        Color rangeColor = (playerInRange && playerDirectlyinFront) ? new Color(0.2f, 1.0f, 0.2f, 0.2f) : new Color(1f, 0.2f, 0.2f, 0.2f);
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

    public override void OnMadeVisibleAgain()
    {
        HitsLeft = initialHitsTillDestroyed;
        bodyAnimations.Play("default");
        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0;
    }
}