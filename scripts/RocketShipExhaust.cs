using Godot;

/// <summary>
/// Controls the ships exhaust appearance.
/// Wrapper class for the trail.
/// As well has the length of the exhaust blast stuff.
/// </summary>
public partial class RocketShipExhaust : PixeliseShaderUpdateSize
{
	[Export] private ColorRect rocketFire;

    private float maxHeight = 3.0f;

    private ShaderMaterial rocketFireMaterial;

    private float maxEnergy;

    [Export] private PointLight2D light2d;

    public override void _Ready()
    {
        base._Ready();
        rocketFireMaterial = rocketFire.Material as ShaderMaterial;
        maxEnergy = light2d.Energy;
    }

    public void UpdateVisualsFromSpeedPercent(float percentageOfFullSpeed)
    {
        if (rocketFireMaterial == null)
        {
            return;
        }

        rocketFireMaterial.SetShaderParameter("flame_height", (maxHeight * percentageOfFullSpeed));
        light2d.Energy = (maxEnergy * percentageOfFullSpeed);
    }
}
