using Godot;
using System;

public partial class Game : Node2D
{
    private Ship ship;

    [Export]
    public PackedScene ShipScene;

    public override void _Ready()
    {
        ship = ShipManager.Instance.CurrentShip;

        if (ship == null)
        {
            ship = ShipScene.Instantiate<Ship>();
            AddChild(ship);

            ShipManager.Instance.SetShip(ship);

            GD.Print("New ship instantiated and assigned to ShipManager.");
        }
        else
        {
            if (ship.GetParent() != this)
            {
                ship.Reparent(this);
            }

            GD.Print("Existing ship loaded from ShipManager.");
        }

        ship.Position = new Vector2(0, 0);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.B && keyEvent.Pressed && !keyEvent.Echo)
            {
                ShipManager.Instance.SetShip(ship);
                GetTree().ChangeSceneToFile("res://ShipBuilder/ShipBuilder.tscn");
            }
        }
    }
}
