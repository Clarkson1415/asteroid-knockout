using Godot;

public partial class PauseMenu : TextureRect
{
    public override void _Ready()
    {
        VisibilityChanged += Sync;
    }

    private void Sync()
    {
        Godot.Engine.TimeScale = this.Visible ? 0 : 1;
    }

    public override void _Process(double delta)
    {
        // TODO: not pausing game when i paus ewith controller :(
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion)
        {
            return;
        }

        if (Input.IsActionJustPressed("pause"))
        {
            if (this.Visible)
            {
                this.Visible = false;
                Godot.Engine.TimeScale = 1;
            }
            else
            {
                this.Visible = true;
                Godot.Engine.TimeScale = 0;
            }
        }
    }
}
