using Godot;

public partial class GlobalSignalBus : Node
{
    private static GlobalSignalBus instance;

    public static GlobalSignalBus GetInstance()
    {
        if (instance == null)
        {
            instance = new GlobalSignalBus();
        }

        return instance;
    }

    [Signal] public delegate void OnAstroidDestroyedEventHandler();

    public void EmitOnAstroidDestroyedSignal()
    {
        EmitSignal(SignalName.OnAstroidDestroyed);
    }

    [Signal] public delegate void OnShipDestroyedEventHandler();

    public void EmitShipDestroyed()
    {
        EmitSignal(SignalName.OnShipDestroyed);
    }
}