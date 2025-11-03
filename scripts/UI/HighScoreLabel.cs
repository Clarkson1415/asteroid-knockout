using Godot;
using System;

public partial class HighScoreLabel : Label
{
	public void UpdateHighScoreText()
	{
		var highScore = Level.GetHighScore();
		var timeForAnimation = 0.3f;
		var timer = new Timer();
		AddChild(timer);
		timer.WaitTime = timeForAnimation / highScore;

		var number = 0;
		timer.Timeout += () => 
		{
			if (number == highScore)
			{
                timer.Stop(); // IDK if this means the timer will stop replaying if stop() is called and its not a one shot.
                return;
			}

			Text = number.ToString(); 
			number++; 
		};

		timer.Start();
    }
}
