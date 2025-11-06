using Godot;

public partial class QuitButton : Button
{
    public override void _Pressed()
    {
        GetTree().Quit();
    }
}
