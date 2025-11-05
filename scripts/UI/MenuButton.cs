using Godot;
using System;

[GlobalClass]
public partial class MenuButton : Button
{
	[Export] public MenuScreen menuToOpen;
}
