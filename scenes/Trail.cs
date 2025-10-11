using Godot;

public partial class Trail : Line2D
{
	[Export] private int length = 50;

	private bool on = false;
	
	private Vector2 point;
	
	public void TurnOff()
	{
		ClearPoints();
		on = false;
    }

	public void TurnOn()
	{
		on = true;
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		if (!on)
		{
			return;
		}

		GlobalPosition = Vector2.Zero;
		GlobalRotation = 0;
		point = GetParent<Node2D>().GlobalPosition;
		AddPoint(point);
		
		while(GetPointCount() > length)
		{
			RemovePoint(0);
		}
    }
}
