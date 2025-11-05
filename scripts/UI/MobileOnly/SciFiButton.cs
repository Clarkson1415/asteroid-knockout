using Godot;
using System;
using System.Collections.Generic;

public partial class SciFiButton : TouchScreenButton
{
    [Export]
    public float RotationSpeed = 2.0f; // Radians per second

    [Export]
    public Godot.Collections.Array<Node2D> Segments = new Godot.Collections.Array<Node2D>();

    private List<float> targetAngles = new List<float>();
    private Random rand = new Random();

    public override void _Ready()
    {
        if (Segments.Count == 0)
        {
            // Auto-assign all children as segments
            foreach (var child in GetChildren())
            {
                if (child is Node2D node)
                    Segments.Add(node);
            }
        }


        this.Pressed += () =>
        {
            GenerateNewTargetAngles();
        };
    }

    public override void _Process(double delta)
    {
        if (Segments.Count != targetAngles.Count) 
        {
            return;
        }

        for (int i = 0; i < Segments.Count; i++)
        {
            var seg = Segments[i];
            float current = seg.Rotation;
            float target = targetAngles[i];

            seg.Rotation = Mathf.LerpAngle(current, target, (float)delta * RotationSpeed);
        }
    }

    private void GenerateNewTargetAngles()
    {
        targetAngles.Clear();
        foreach (var segment in Segments)
        {
            float angle = (float)(rand.NextDouble() * Mathf.Tau); // Random angle between 0 and 2π
            targetAngles.Add(angle);
        }
    }
}
