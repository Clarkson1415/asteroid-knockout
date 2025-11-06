using Godot;
using System;
using System.Linq;

public partial class RemappingButton : Button
{
	[Export] private string actionRemapName = "boost"; // fire or boost
	[Export] private Label labelDisplaying;
	private bool waitingForNewMap;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		this.ButtonUp += Remap;
        this.VisibilityChanged += SyncButtonNames;
	}

    private void SyncButtonNames()
    {
        InputEvent eventForThisAction = InputMap.ActionGetEvents(actionRemapName).FirstOrDefault();
        if (eventForThisAction == null)
        {
            Logger.LogError($"no button mapped to event {actionRemapName}");
            return;
        }

        labelDisplaying.Text = GetDisplayNameOfInput(eventForThisAction);
    }

    private void Remap()
	{
        waitingForNewMap = true;
        labelDisplaying.Text = "waiting for input...";
        Text = "...";
    }

    private string GetDisplayNameOfInput(InputEvent @event)
    {
        var buttonName = "?";

        if (@event is InputEventKey keyEvent)
        {
            Key keyEnum = keyEvent.Keycode;
            buttonName = OS.GetKeycodeString(keyEnum);
        }
        else if (@event is InputEventJoypadButton buttonEvent)
        {
            int buttonIndex = (int)buttonEvent.ButtonIndex;

            // Get the device ID (incase multiple controllers are connected)
            //int deviceId = buttonEvent.Device;

            buttonName = GetJoypadButtonName(buttonIndex);
        }
        else if (@event is InputEventMouseButton mouseClick)
        {
            buttonName = mouseClick.ButtonIndex switch
            {
                MouseButton.Right => "Right Click",
                MouseButton.Left => "Left Click",
                MouseButton.Middle => "Middle Click",
                _ => throw new Exception("shit")
            };
        }

        return buttonName;
    }

    public override void _Input(InputEvent @event)
    {
		if (!waitingForNewMap) { return; }

        if (@event is InputEventMouseMotion)
        {
            return;
        }

		InputMap.ActionAddEvent(actionRemapName, @event);

        labelDisplaying.Text = "...";

        // Get Name of input.
        labelDisplaying.Text = GetDisplayNameOfInput(@event);
        waitingForNewMap = false;
        Text = "remap";

        GameSettings.Instance.UpdateInputMappings(actionRemapName, @event);
    }

    private string GetJoypadButtonName(int buttonIndex)
    {
        // Attempt to convert the integer back to the JoyButton enum name
        if (Enum.IsDefined(typeof(Godot.JoyButton), buttonIndex))
        {
            return Enum.GetName(typeof(Godot.JoyButton), buttonIndex);
        }

        return $"Unknown Button ({buttonIndex})";
    }
}
