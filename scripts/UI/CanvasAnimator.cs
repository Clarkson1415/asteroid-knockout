using Godot;
using System;

public partial class CanvasAnimator : AnimationPlayer
{
	private string gameOver = "gameOver";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		GlobalSignalBus.GetInstance().OnShipDestroyed += GameOver;
	}

    public override void _ExitTree()
    {
        GlobalSignalBus.GetInstance().OnShipDestroyed -= GameOver;
    }

    private void GameOver()
	{
        if (CurrentAnimation == gameOver)
		{
			return;
		}

		Play(gameOver);
	}
}
