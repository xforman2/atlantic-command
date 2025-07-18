using Godot;

public partial class InputManager : Node
{
	public override void _UnhandledInput(InputEvent @event)
	{
		GD.Print("test");
		if (@event.IsActionPressed("ui_cancel"))
		{
			GetTree().ChangeSceneToFile("res://MainMenu/MainMenu.tscn");
		}
	}
}
