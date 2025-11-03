using Godot;

public partial class BoostAmount : Control
{
    public override void _Ready()
    {
        this.Material = this.Material.Duplicate() as ShaderMaterial;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        var player = ShipController.Instance;

        if (player == null) { return; }

        var mat = this.Material as ShaderMaterial;
        var boostRemaining = player.BoostPercentRemaining();
        mat.SetShaderParameter("fade_fill_amount", boostRemaining);
    }
}
