using Godot;
using System;

public partial class Dock : StaticBody2D
{

    [Export]
    public Vector2I SpawnPosition;
    private Button _buildButton;

    public override void _Ready()
    {
        var dockArea = GetNode<Area2D>("DockArea");
        dockArea.BodyEntered += OnBodyEntered;
        dockArea.BodyExited += OnBodyExited;

        _buildButton = GetTree()
            .Root
            .GetNode<Node2D>("Game")
            .GetNode<CanvasLayer>("GameOverlay")
            .GetNode<Button>("BuildButton");
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Ship)
        {
            _buildButton.Visible = true;
            ShipManager.Instance.SetShipWorldPosition(SpawnPosition);
        }
    }

    private void OnBodyExited(Node body)
    {
        if (body is Ship)
        {
            _buildButton.Visible = false;
        }
    }
}
