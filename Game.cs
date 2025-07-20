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
        if (_ship == null)
        {
            _ship = ShipScene.Instantiate<Ship>();
            _ship.Init();
            AddChild(_ship);

            ShipManager.Instance.SetShip(_ship);

            GD.Print("New ship instantiated and assigned to ShipManager.");
        }
        else
        {
            if (_ship.GetParent() != this)
            {
                _ship.Reparent(this);
            }

            GD.Print("Existing ship loaded from ShipManager.");
        }

        _ship.Position = new Vector2(0, 0);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.B && keyEvent.Pressed && !keyEvent.Echo)
            {
                ShipManager.Instance.SetShip(_ship);
                _ship.GoToDock();
                GetTree().ChangeSceneToFile("res://ShipBuilder/ShipBuilder.tscn");
            }
        }
    }

    private void OnButtonPressed()
    {
        _ship.playerResourceManager.DecreaseWood(10);
    }
}
