using Godot;
using System;

public partial class GetHitComponent : Area2D
{
    private ShaderMaterial flashMaterial;

    private Timer flashTimer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        AreaEntered += OnHit;
	}

    private void OnHit(Area2D area)
    {
        Flash();
    }

	public void Flash()
	{
        if (flashTimer != null)
        {
            if (!flashTimer.IsStopped())
            {
                return;
            }
        }

        if (flashMaterial == null)
        {
            return;
        }

        // FLASH
        flashTimer = new Timer();
        flashTimer.OneShot = true;
        AddChild(flashTimer);
        flashTimer.WaitTime = 0.1f;
        flashTimer.Timeout += () => flashMaterial.SetShaderParameter("flash_strength", 0f);
        flashTimer.Start();

        flashMaterial.SetShaderParameter("flash_strength", 1f);
    }
}
