using Godot;

public partial class Engine : Node2D
{
	[Export] AnimationPlayer animationPlayer;

    private string idle = "idle";
    private string powered = "powered";
    private string powerOn = "poweringOn";
    private string powerOff = "poweringOff";
    private string boost = "boost";

    [Export] RocketShipExhaust exhastLeft;
    [Export] RocketShipExhaust exhastRight;

    private void UpdateExhastVisuals(float percentageSpeed)
    {
        exhastLeft.UpdateVisualsFromSpeedPercent(percentageSpeed);
        exhastRight.UpdateVisualsFromSpeedPercent(percentageSpeed);
    }

    public void TurnOffSprites()
    {
        foreach (var c in GetChildren())
        {
            if (c is AnimatedSprite2D animsprite)
            {
                animsprite.Visible = false;
            }
        }

        exhastLeft.Visible = false;
        exhastRight.Visible = false;
    }

    public override void _Ready()
    {
        animationPlayer.AnimationFinished += AnimationFinished;
    }

    public void Boost()
    {
        UpdateExhastVisuals(1.0f);

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
        UpdateExhastVisuals(0.5f);

        if (animationPlayer.CurrentAnimation == powerOn || animationPlayer.CurrentAnimation == powered)
        {
            return;
        }

        animationPlayer.Play(powerOn);
    }

    public void PowerOff()
	{
        UpdateExhastVisuals(0.1f);

        if (animationPlayer.CurrentAnimation == idle)
        {
            return;
        }

        animationPlayer.Play(idle);
    }
}
