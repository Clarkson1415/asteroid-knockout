using Godot;

/// <summary>
/// The player.
/// </summary>
public partial class ShipController : RigidBody2D
{
    [Export] private Area2D pickupCollectionArea;

    [Export] protected float Acceleration = 300f;

    [Export] private float _maxSpeed = 100f;

    public float MaxSpeed
    {
        get { return this._maxSpeed; }
        private set { _maxSpeed = value; }
    }
    
    [Export] private FollowingCamera2D camera2d;

    /// <summary>
    /// How fast boost accellerates.
    /// </summary>
    [Export] private float boostAccellerationMultiplier = 2f;

    [Export] public Node JoystickLeft;

    [Export] private TouchScreenButton fireButton;
    [Export] private TouchScreenButton boostButton;

    [Export] private Area2D area;

    // TODO: these will be able to be changed at runtime when picked up.
    [Export] private Engine currentEngine;
    [Export] private Weapon currentWeapon;

    private Vector2 inputVector;

    enum DAMAGE { none = 0, slight = 1, half = 2, completely = 3}

    private DAMAGE damageLevel = DAMAGE.none;

    [Export] Godot.Collections.Dictionary<DAMAGE, Texture2D> damageSprites;

    [Export] AnimatedSprite2D shipSprite;

    private bool isBoosting = false;

    [Export] private AnimationPlayer shipSpriteAnimator;

    private string explode = "explode";

    private ShaderMaterial flashMaterial;

    [Export] AudioStreamPlayer2D BonkAsteroidSFX;

    public bool IsDead => shipDestroyedHasBeenEmitted;

    private static ShipController _instance;

    public static ShipController Instance { get {  return _instance; } private set { _instance = value; } }

    public override void _Ready()
    {
        BoostRemaining = MaximumBoostAmount;

        Instance = this;

        // TODO: put the hit and flash in the hit component. to do itself.
        area.AreaEntered += OnGotHit;
        CanSleep = false;
        shipSprite.SpriteFrames.SetFrame("default", 0, damageSprites[damageLevel]);

        // duplicate material future proof if add more ships or something idk.
        flashMaterial = (ShaderMaterial)Material.Duplicate();
        Material = flashMaterial;

        pickupCollectionArea.AreaEntered += OnPickupBoost;
    }

    private void OnPickupBoost(Area2D pickupCollected)
    {
        BoostRemaining += MaximumBoostAmount/4;
        var boost = pickupCollected as Boost;
        if (boost != null)
        {
            boost.InvokeDestroyedPoolableObject();
        }
    }

    public override void _ExitTree()
    {
        area.AreaEntered -= OnGotHit;
    }

    /// <summary>
    /// TODO: to be used for pickups.
    /// </summary>
    /// <param name="newWeapon"></param>
    private void AddWeapon(Weapon newWeapon)
    {
        // TODO:
    }

    private void OnGotHit(Area2D area)
    {
        if (shipSpriteAnimator.CurrentAnimation == explode || shipDestroyedHasBeenEmitted)
        {
            return;
        }

        BonkAsteroidSFX.Play();

        // TODO: instead of isBoosting check for the speed of the collision.
        // fully destroy or normal was hit.
        if (damageLevel == DAMAGE.completely)
        {
            currentEngine.PowerOff();
            CameraShaker.LargeShake(camera2d);
            shipSpriteAnimator.Play(explode);
            return;
        }
        else
        {
            CameraShaker.MediumShake(camera2d);
        }

        // if flashing rn ignore
        // TODO: multiple pulsing flashes for a second. 
        // TODO fire damage sprite effects and sound.
        if (flashTimer != null)
        {
            if (!flashTimer.IsStopped())
            {
                return;
            }
        }

        // FLASH
        flashTimer = new Timer();
        flashTimer.OneShot = true;
        AddChild(flashTimer);
        flashTimer.WaitTime = 0.1f;
        flashTimer.Timeout += () => flashMaterial.SetShaderParameter("flash_strength", 0f);
        flashTimer.Start();

        flashMaterial.SetShaderParameter("flash_strength", 1f);

        // sprite change
        damageLevel += 1;
        shipSprite.SpriteFrames.SetFrame("default", 0, damageSprites[damageLevel]);
    }

    private Timer flashTimer;

    private bool shipDestroyedHasBeenEmitted;

    /// <summary>
    /// TODO: to be called from animation player after animation of ship exploding.
    /// </summary>
    public void OnShipDestroyed()
    {
        if (shipDestroyedHasBeenEmitted)
        {
            return;
        }

        shipDestroyedHasBeenEmitted = true;
        GlobalSignalBus.GetInstance().EmitShipDestroyed();
    }

    [Export] private float BoostUsedPerSec = 1;

    [Export] private float RegenPerSecNotUsing = 1;

    private float BoostRemaining = 100;

    [Export] private float MaximumBoostAmount = 100;

    public float BoostPercentRemaining()
    {
        return BoostRemaining / MaximumBoostAmount;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (currentEngine != null)
        {
            var percent = LinearVelocity.Length() / MaxSpeed;
        }

        if (isBoosting)
        {
            BoostRemaining -= (float)(BoostUsedPerSec * delta);
            if (BoostRemaining < 0) // stop at 0
            {
                BoostRemaining = 0;
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (GlobalSignalBus.MenusOpen)
        {
            return;
        }

        float dt = (float)delta;

        if (fireButton.IsPressed() || Input.IsActionPressed("fire"))
        {
            currentWeapon.Fire();
        }

        isBoosting = (boostButton.IsPressed() || Input.IsActionPressed("boost")) && (BoostRemaining > 0);

        if (shipSpriteAnimator.CurrentAnimation == explode || shipDestroyedHasBeenEmitted)
        {
            return;
        }

        Movement(dt);

        // Engine SFX
        if (isBoosting)
        {
            currentEngine.Boost();
        }
        else if (inputVector != Vector2.Zero)
        {
            currentEngine.PowerOn();
        }
        else
        {
            currentEngine.PowerOff();
        }
    }

    private void Movement(float dt)
    {
        inputVector = Input.GetVector("left", "right", "up", "down");

        if (JoystickLeft != null && (bool)JoystickLeft.Get("is_pressed"))
        {
            inputVector = (Vector2)JoystickLeft.Get("output");
        }

        // change dir only if input
        if (inputVector != Vector2.Zero)
        {
            GlobalRotation = inputVector.Angle() + Mathf.Pi / 2;
        }

        var facingDirection = new Vector2(Mathf.Sin(GlobalRotation), -Mathf.Cos(GlobalRotation));

        if (isBoosting)
        {
            ApplyCentralForce(facingDirection.Normalized() * Acceleration * boostAccellerationMultiplier);
        }
        else if (inputVector != Vector2.Zero)
        {
            ApplyCentralForce(facingDirection.Normalized() * Acceleration);
        }

        // Clamp max speed
        if (LinearVelocity.Length() > MaxSpeed)
        {
            LinearVelocity = LinearVelocity.Normalized() * MaxSpeed;
        }
    }
}
