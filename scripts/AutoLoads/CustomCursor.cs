using Godot;

public partial class CustomCursor : Node
{
    string defaultTexturePath = "res://cursors/pointer34x34.png";

    public override void _Ready()
    {
        // Load the texture
        var cursorTexture = ResourceLoader.Load<Texture2D>(defaultTexturePath);
        if (cursorTexture != null)
        {
            // Set as the default mouse cursor
            Input.SetCustomMouseCursor(cursorTexture);
            
            // Optional: set for a specific cursor shape
            // Input.SetCustomMouseCursor(cursorTexture, Input.CursorShape.Ibeam);
        }
        else
        {
            GD.PrintErr("Failed to load cursor texture at " + defaultTexturePath);
        }
    }
}
