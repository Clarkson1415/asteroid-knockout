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

		GlobalSignalBus.GetInstance().OnAstroidDestroyed += OnDestroyed;
    }

    public override void _ExitTree()
    {
        GlobalSignalBus.GetInstance().OnAstroidDestroyed -= OnDestroyed;
    }

    private void IncreaseAsteroidSpeed()
	{
		astroidSpawner.IncreaseSpeeds(10f * (float)levels);
        levels++;
    }

	private void OnDestroyed()
	{
		astroidsDestroyed++;
		astroidsScore.Text = astroidsDestroyed.ToString();
    }
}
