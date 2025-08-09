using Godot;

public partial class InputManager : Node
{
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            var ship = ShipManager.Instance.CurrentShip;
            ship.DisableCamera();
            ShipManager.Instance.ReparentShip(ship);
            GetTree().ChangeSceneToFile("res://MainMenu/MainMenu.tscn");
        }
    }
}
