using Godot;
using Godot.Collections;

public partial class PauseMenuController : MenuController
{
    [Export] public Array<MenuButton> buttonsThatOpenSubmenus;

    public override void _Ready()
    {
        base._Ready();

        // close when the first menu button closes it.
        initial.backButton.Pressed += () => { this.Visible = false; Sync(); };

        foreach (var b in buttonsThatOpenSubmenus)
        {
            b.Pressed += () => this.ChangeToMenu(b.menuToOpen);
        }

        this.Visible = false;
    }

    private void ChangeToMenu(MenuScreen newMenu)
    {
        current = newMenu;
        newMenu.Open();

        current.backButton.Pressed += () => CloseCurrent();
    }

    private void CloseCurrent()
    {
        current.Close();
        current = null;
    }

    private void Sync()
    {
        Godot.Engine.TimeScale = this.Visible ? 0 : 1;
        GlobalSignalBus.MenusOpen = this.Visible;

        foreach (var s in screens)
        {
            s.Close();
        }

        if (this.Visible) // just turned on.
        {
            initial.Open();
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if (Input.IsActionJustPressed("pause"))
        {
            this.Visible = !this.Visible; 
            Sync();
        }
    }
}
