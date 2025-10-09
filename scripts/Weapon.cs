using Godot;
using System;

/// <summary>
/// One of the weapons in the game.
/// </summary>
public partial class Weapon : AnimatedSprite2D
{
	[Export] private AnimationPlayer animationPlayer;

	// basic auto does 2 every 0.5 seconds. as 5 frames at 10 fps. 5/10 = 0.5 seconds per 2 bullets. 4 per 1 second.

	private int bulletsPerSecond = 4;

	[Export] private PackedScene bulletScene;

	[Export] private Node2D leftMuzzle;
	[Export] private Node2D rightMuzzle;

    private CameraShake camera;

    public override void _Ready()
    {
        animationPlayer.AnimationFinished += OnAnimationFinished;
    }

    public void SetCamera(CameraShake camFromTop)
    {
        camera = camFromTop;
    }

    private void OnAnimationFinished(StringName anim)
    {
        if (anim == "fire")
        {
            animationPlayer.Play("idle");
        }
    }

    public void Fire()
	{
		if (animationPlayer.IsPlaying() && animationPlayer.CurrentAnimation == "fire")
		{
			return;
        }
		else
		{
            animationPlayer.Play("fire");
        }
    }

	// for the animation player to call.
	public void InstantiateBulletLeft()
	{
        camera.Shake(0.1f, 5);

        var bullet = (Bullet)bulletScene.Instantiate();
        GetTree().CurrentScene.AddChild(bullet);
        bullet.GlobalPosition = leftMuzzle.GlobalPosition;
        bullet.GlobalRotation = GlobalRotation;
    }

    public void InstantiateBulletRight()
	{
        camera.Shake(0.1f, 5);

        var bullet = (Bullet)bulletScene.Instantiate();
        GetTree().CurrentScene.AddChild(bullet);
        bullet.GlobalPosition = rightMuzzle.GlobalPosition;
        bullet.GlobalRotation = GlobalRotation;
    }
}
