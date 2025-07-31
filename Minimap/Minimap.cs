using Godot;
using System;
public partial class Minimap : SubViewport
{
    private RigidBody2D _ship;
    private Camera2D _camera;

    public override void _Ready()
    {
        _ship = GetTree().Root.GetNode<RigidBody2D>("Game/Ship");
        _camera = GetNode<Camera2D>("MinimapCamera");
        World2D = (World2D)GetTree().Root.World2D;


        if (_camera != null)
        {
            _camera.Zoom = new Vector2(0.1f, 0.1f);
        }

    }

    public override void _PhysicsProcess(double delta)
    {
        if (_ship != null && _camera != null)
        {
            _camera.Position = _ship.Position;
        }
    }
}
