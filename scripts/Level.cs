using Godot;

public partial class Level : Node
{
	/// <summary>
	/// every time this runs out level gets harder
	/// </summary>
	private float levelIncrementTime = 10f;

	[Export] private AstroidSpawner astroidSpawner;

	[Export] private Label astroidsScore;

    /// <summary>
    /// Level increments survived.
    /// </summary>
    private int levels = 0;

	private int asteroids = 0;

	public override void _Ready()
	{
		var timer = new Timer();
		AddChild(timer);
		timer.WaitTime = levelIncrementTime;
		timer.Timeout += IncreaseAsteroidSpeed;

		GlobalSignalBus.GetInstance().OnAstroidDestroyed += OnDestroyedAsteroid;
		GlobalSignalBus.GetInstance().OnShipDestroyed += OnShipDestroyed;
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

	private void OnShipDestroyed()
	{
        GlobalSignalBus.GetInstance().OnAstroidDestroyed -= OnDestroyedAsteroid;

        if (asteroids <= GameSettings.Instance.GetHighScore())
		{
			return;
		}

		GameSettings.Instance.UpdateHighscore(asteroids);
	}

	private void OnDestroyedAsteroid()
	{
		asteroids++;
		astroidsScore.Text = $"Asteroids: {(asteroids.ToString())}";
    }
}
