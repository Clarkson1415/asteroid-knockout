using Godot;

public partial class Level : Node
{
	/// <summary>
	/// every time this runs out level gets harder
	/// </summary>
	private float levelIncrementTime = 10f;

	[Export] private AstroidSpawner astroidSpawner;

	[Export] private Label astroidsScore;

	// TODO: have time lasted
	private static ConfigFile Config = new ConfigFile();

	private static string filePath = "user://scores.cfg";

    /// <summary>
    /// Level increments survived.
    /// </summary>
    private int levels = 0;

	private int astroidsDestroyed = 0;

	public override void _Ready()
	{
		var timer = new Timer();
		AddChild(timer);
		timer.WaitTime = levelIncrementTime;
		timer.Timeout += IncreaseAsteroidSpeed;

		GlobalSignalBus.GetInstance().OnAstroidDestroyed += OnDestroyedAsteroid;
		GlobalSignalBus.GetInstance().OnShipDestroyed += OnShipDestroyed;

		Error err = Config.Load(filePath);
    }

    public override void _ExitTree()
    {
        GlobalSignalBus.GetInstance().OnShipDestroyed += OnShipDestroyed;
        GlobalSignalBus.GetInstance().OnAstroidDestroyed -= OnDestroyedAsteroid;
    }

    private void IncreaseAsteroidSpeed()
	{
		astroidSpawner.IncreaseSpeeds(10f * (float)levels);
        levels++;
    }

    private class SaveDataSections
    {
        public static string playerData = "playerData";
    }

    private class SaveDataKeys
	{
		public static string highscore = "highscore";
	}

	private void OnShipDestroyed()
	{
		// unsub after ship destroyed tho.
        GlobalSignalBus.GetInstance().OnAstroidDestroyed -= OnDestroyedAsteroid;

        if (astroidsDestroyed <= GetHighScore())
		{
			return;
		}

		Config.SetValue(SaveDataSections.playerData, SaveDataKeys.highscore, astroidsDestroyed);
		Config.Save(filePath);
	}

	private void OnDestroyedAsteroid()
	{
		astroidsDestroyed++;
		astroidsScore.Text = $"Asteroids: {(astroidsDestroyed.ToString())}";
    }

	public static int GetHighScore()
	{
		return (int)Config.GetValue(SaveDataSections.playerData, SaveDataKeys.highscore);
	}
}
