using Godot;
using Godot.Collections;

public partial class Engine : Node2D
{
	[Export] AnimationPlayer animationPlayer;

    private string idle = "idle";
    private string powered = "powered";
    private string powerOn = "poweringOn";
    private string powerOff = "poweringOff";
    private string boost = "boost";

    [Export] GpuParticles2D emissionLeft;
    [Export] GpuParticles2D emissionRight;

    public void TurnOffSprites()
    {
        foreach (var c in GetChildren())
        {
            if (c is AnimatedSprite2D animsprite)
            {
                animsprite.Visible = false;
            }
        }

        emissionLeft.Emitting = false;
        emissionRight.Emitting = false;
    }

    public override void _Ready()
    {
        animationPlayer.AnimationFinished += AnimationFinished;
    }

    public void Boost()
    {
        if (animationPlayer.CurrentAnimation == boost)
        {
            return;
        }

        animationPlayer.Play(boost);
    }

	private void AnimationFinished(StringName animName)
	{
        if (animName == powerOn)
        {
            animationPlayer.Play(powered);
        }
        else if (animName == powerOff)
        {
            animationPlayer.Play(idle);
        }
    }

	public void PowerOn()
	{
        if (animationPlayer.CurrentAnimation == powerOn || animationPlayer.CurrentAnimation == powered)
        {
            return;
        }

        animationPlayer.Play(powerOn);
    }

    public void PowerOff()
	{
        if (animationPlayer.CurrentAnimation == idle)
        {
            return;
        }

        animationPlayer.Play(idle);
    }
}
