using Godot;
using System;

public partial class HighScoreLabel : Label
{
	public void UpdateHighScoreText()
	{
		Text = Level.GetHighScore().ToString();
    }
}
