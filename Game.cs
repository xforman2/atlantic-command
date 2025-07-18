using Godot;
using System;

public partial class Game : Node2D
{
    private Ship _ship;
    private Button _button;

    [Export]
    public PackedScene ShipScene;

    public override void _Ready()
    {
        _button = GetNode<Button>("Button");
        _button.Pressed += OnButtonPressed;
        _ship = ShipManager.Instance.CurrentShip;
        GD.Print("test hey");

        if (_ship == null)
        {
            _ship = ShipScene.Instantiate<Ship>();
            AddChild(_ship);
        }
        else
        {
            if (_ship.GetParent() != this)
            {
                _ship.Reparent(this);
            }

        }

    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.B && keyEvent.Pressed && !keyEvent.Echo)
            {
                ShipManager.Instance.SetShip(_ship);
                _ship.Position = new Vector2(0, 0);

                GD.Print(_ship.Position);
                GetTree().ChangeSceneToFile("res://ShipBuilder/ShipBuilder.tscn");
            }
        }
    }

    private void OnButtonPressed()
    {
        _ship._resourceManager.DecreaseWood(10);

    }
}
