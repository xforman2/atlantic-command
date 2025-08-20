using Godot;

public partial class InputManager : Node
{
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            var ship = ShipManager.Instance.CurrentShip;

            if (ship is not null)
            {

                if (ship.IsDestroyed())
                {
                    GameState.Instance.LastOrigin = SceneOrigin.ShipBuilder;
                    ship.QueueFree();
                    ShipManager.Instance.CurrentShip = null;
                }
                else
                {
                    ship.DisableCamera();
                    ShipManager.Instance.ReparentShip(ship);
                }
            }
            GetTree().ChangeSceneToFile("res://MainMenu/MainMenu.tscn");
        }
    }
}
