using Godot;
using System;
using System.Linq;
using static System.Collections.Specialized.BitVector32;

/// <summary>
/// Autoload.
/// Contains game visual settings, save data etc.
/// </summary>
public partial class GameSettings : Node
{
    private static ConfigFile Config = new ConfigFile();

    // TODO: save visual settings, score etc. in the config.
    private static string filePath = "user://scores.cfg";

    public bool TouchControlsOn = false;

	/// <summary>
	/// FPS and stuff.
	/// </summary>
	public bool debugOn = false;

    public static GameSettings Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        Error err = Config.Load(filePath);

        // Check if any key exists in the defaultInputMappings section
        bool defaultMappingsExist = Config.GetSections().Contains(SaveDataKeys.defaultInputMappings);
        if (!defaultMappingsExist)
        {
            SetDefaultInputMappings();
        }

        // if there is custom input mappings load those instead.
        bool customMappingsExist = Config.GetSections().Contains(SaveDataKeys.customInputMappings);
        if (!customMappingsExist)
        {
            LoadCustomInputMappings();
        }
    }

    private void SetDefaultInputMappings()
    {
        var actions = InputMap.GetActions();
        foreach (var a in actions)
        {
            var defaultKeybind = InputMap.ActionGetEvents(a).FirstOrDefault();
            SaveInputMappingToConfig(SaveDataKeys.defaultInputMappings, a, defaultKeybind);
        }
    }

    private class SaveDataSections
    {
        public static string playerData = "playerData";
    }

    private class SaveDataKeys
    {
        // TODO: add graphics settings preferences here too Vsync and stuff.

        public static string highscore = "highscore";
        public static string customInputMappings = "customInputMappings";
        public static string defaultInputMappings = "defaultInputMappings";
    }

    public static int GetHighScore()
    {
        return (int)Config.GetValue(SaveDataSections.playerData, SaveDataKeys.highscore);
    }

    public static void UpdateHighscore(int astroidsDestroyed)
    {
        Config.SetValue(SaveDataSections.playerData, SaveDataKeys.highscore, astroidsDestroyed);
        Config.Save(filePath);
    }

    /// <summary>
    /// Add new input map to config remove the old for that action.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="newKeybind">The new key.</param>
    public static void UpdateInputMappings(string action, InputEvent newKeybind)
    {
        // Clear old events
        var events = InputMap.ActionGetEvents(action);
        foreach (var ev in events)
            InputMap.ActionEraseEvent(action, ev);

        SaveInputMappingToConfig(SaveDataKeys.customInputMappings, action, newKeybind);
    }

    private static void SaveInputMappingToConfig(string saveDataKey, string action, InputEvent newKeybind)
    {
        // Add new event
        InputMap.ActionAddEvent(action, newKeybind);

        // Save to config
        if (newKeybind is InputEventKey key)
        {
            Config.SetValue(SaveDataSections.playerData, $"{saveDataKey}_{action}_type", "key");
            Config.SetValue(SaveDataSections.playerData, $"{saveDataKey}_{action}_scancode", (int)key.Keycode);
        }
        else if (newKeybind is InputEventMouseButton mouse)
        {
            Config.SetValue(SaveDataSections.playerData, $"{saveDataKey}_{action}_type", "mouse");
            Config.SetValue(SaveDataSections.playerData, $"{saveDataKey}_{action}_button", (int)mouse.ButtonIndex);
        }
        else if (newKeybind is InputEventJoypadButton joy)
        {
            Config.SetValue(SaveDataSections.playerData, $"{saveDataKey}_{action}_type", "joy");
            Config.SetValue(SaveDataSections.playerData, $"{saveDataKey}_{action}_button", (int)joy.ButtonIndex);
            Config.SetValue(SaveDataSections.playerData, $"{saveDataKey}_{action}_device", joy.Device);
        }

        Config.Save(filePath);
    }

    /// <summary>
    /// Loads input mappings from save onto buttons.
    /// </summary>
    private static void LoadCustomInputMappings()
    {
        foreach (string action in InputMap.GetActions())
        {
            string type = (string)Config.GetValue(SaveDataSections.playerData,
                                                 $"{SaveDataKeys.customInputMappings}_{action}_type",
                                                 "");

            if (string.IsNullOrEmpty(type))
                continue;

            InputEvent ev = null;

            switch (type)
            {
                case "key":
                    int scancode = (int)Config.GetValue(SaveDataSections.playerData,
                                                        $"{SaveDataKeys.customInputMappings}_{action}_scancode",
                                                        0);
                    ev = new InputEventKey { Keycode = (Key)scancode };
                    break;

                case "mouse":
                    int mouseButton = (int)Config.GetValue(SaveDataSections.playerData,
                                                           $"{SaveDataKeys.customInputMappings}_{action}_button",
                                                           0);
                    ev = new InputEventMouseButton { ButtonIndex = (MouseButton)mouseButton };
                    break;

                case "joy":
                    int joyButton = (int)Config.GetValue(SaveDataSections.playerData,
                                                         $"{SaveDataKeys.customInputMappings}_{action}_button",
                                                         0);
                    int device = (int)Config.GetValue(SaveDataSections.playerData,
                                                      $"{SaveDataKeys.customInputMappings}_{action}_device",
                                                      0);
                    ev = new InputEventJoypadButton { ButtonIndex = (JoyButton)joyButton, Device = device };
                    break;
            }

            if (ev != null)
                InputMap.ActionAddEvent(action, ev);
        }
    }
}
