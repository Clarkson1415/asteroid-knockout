using Godot;
using System.Linq;

/// <summary>
/// Autoload.
/// Contains game visual settings, save data etc.
/// </summary>
public partial class GameSettings : Node
{
    #region userPreferences
    /// <summary>
	/// Value from 0 to 1. 0 being no camera shake. TODO: setup settings ui for this.
	/// </summary>
	public double GLOBAL_CAMERA_SHAKE_INTENSITY { get; private set; }

    private bool _vsyncOn = true;

    public bool VsyncOn 
    {
        get 
        { 
            return _vsyncOn; 
        }

        private set
        {
            _vsyncOn = value;
            if (_vsyncOn)
            {
                // TODO: Vsync setting should be any of the enums available there instead of off on.
                Logger.Log("TODO Vsync setting should be any of the enums available there instead of off on.");
                DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
            }
            else
            {
                DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
            }
        } 
    }

    #endregion

    private ConfigFile Config = new ConfigFile();

    // TODO: save visual settings, score etc. in the config.
    private string filePath = "user://game_data.cfg";

    public bool TouchControlsOn = true;

	/// <summary>
	/// FPS and stuff.
	/// </summary>
	public bool debugOn = false;

    public static GameSettings Instance { get; private set; }

    /// <summary>
    /// NOTE: Autoload signletons such as this call ready BEFORE any other nodes in the scene tree do!
    /// </summary>
    public override void _Ready()
    {
        Instance = this;
        LoadInSavedData();
    }

    public void LoadInSavedData()
    {
        Error err = Config.Load(filePath);

        Logger.Log("fix this");
        // Check if any key exists in the defaultInputMappings section
        // bool defaultMappingsExist = Config.HasSectionKey(SaveDataSections.playerData, SaveDataKeys.defaultInputMappings);
        var sectionKeys = Config.GetSectionKeys(SaveDataSections.playerData);
        var defaultMappingsExist = sectionKeys.Any(x => x.Contains("default"));
        if (!defaultMappingsExist)
        {
            SetDefaultInputMappings();
        }

        // if there is custom input mappings load those instead.
        var customMappingsExist = sectionKeys.Any(x => x.Contains(SaveDataKeys.defaultInputMappings));
        if (customMappingsExist)
        {
            LoadCustomInputMappings();
        }

        // load user preferences: e.g. camera shake
        GLOBAL_CAMERA_SHAKE_INTENSITY = LoadCameraShakePreference();
        VsyncOn = LoadVsyncPreference();
    }

    private bool LoadVsyncPreference()
    {
        if (!Config.HasSectionKey(SaveDataSections.playerData, SaveDataKeys.vsyncOn))
        {
            return true; // default value
        }

        return (bool)Config.GetValue(SaveDataSections.playerData, SaveDataKeys.vsyncOn);
    }

    public void SaveNewVsyncPreference(bool vsyncOn)
    {
        VsyncOn = vsyncOn;
        Config.SetValue(SaveDataSections.playerData, SaveDataKeys.vsyncOn, vsyncOn);
        Config.Save(filePath);
    }

    private double LoadCameraShakePreference()
    {
        if (!Config.HasSectionKey(SaveDataSections.playerData, SaveDataKeys.cameraShakeAmount))
        {
            return 0.6; // default value
        }

        return (double)Config.GetValue(SaveDataSections.playerData, SaveDataKeys.cameraShakeAmount);
    }

    /// <summary>
    /// Save new value bewteen 0 to 1.
    /// </summary>
    /// <param name="newValue"></param>
    public void SaveNewCameraShakePreference(double newValue)
    {
        GLOBAL_CAMERA_SHAKE_INTENSITY = newValue;
        Config.SetValue(SaveDataSections.playerData, SaveDataKeys.cameraShakeAmount, newValue);
        Config.Save(filePath);
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
        public static string cameraShakeAmount = "cameraShakeAmount";
        public static string vsyncOn = "vsyncOn";

        // TODO: add graphics settings preferences here too Vsync and stuff.
        public static string highscore = "highscore";
        public static string customInputMappings = "customInputMappings";
        public static string defaultInputMappings = "defaultInputMappings";
    }

    public int GetHighScore()
    {
        return (int)Config.GetValue(SaveDataSections.playerData, SaveDataKeys.highscore);
    }

    public void UpdateHighscore(int astroidsDestroyed)
    {
        Config.SetValue(SaveDataSections.playerData, SaveDataKeys.highscore, astroidsDestroyed);
        Config.Save(filePath);
    }

    /// <summary>
    /// Add new input map to config remove the old for that action.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="newKeybind">The new key.</param>
    public void UpdateInputMappings(string action, InputEvent newKeybind)
    {
        // Clear old events
        var events = InputMap.ActionGetEvents(action);
        foreach (var ev in events)
            InputMap.ActionEraseEvent(action, ev);

        SaveInputMappingToConfig(SaveDataKeys.customInputMappings, action, newKeybind);
    }

    private void SaveInputMappingToConfig(string saveDataKey, string action, InputEvent newKeybind)
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
    private void LoadCustomInputMappings()
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
                InputMap.ActionEraseEvents(action);
                InputMap.ActionAddEvent(action, ev);
        }
    }
}
