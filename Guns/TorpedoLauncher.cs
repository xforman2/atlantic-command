using Godot;
using System;
using System.Collections.Generic;

public partial class TorpedoLauncher : Gun
{
    private PackedScene _projectileScene;
    private Marker2D _muzzle;

    public override void _Ready()
    {
        _projectileScene = GD.Load<PackedScene>("res://Projectiles/Torpedo.tscn");
        _muzzle = GetNode<Marker2D>("Muzzle");
    }

    public override void Init(Vector2I origin, List<Vector2I> occupiedPositions, float rotation)
    {
        Origin = origin;

        OccupiedPositions = occupiedPositions;

        Position = origin;
        RotationDegrees = rotation;
        Size = new Vector2I(64, 64);
    }

    public override void Shoot(Vector2? target = null)
    {
        if (_projectileScene == null || _muzzle == null)
        {
            GD.PrintErr("Missing projectile or muzzle.");
            return;
        }

        var projectile = _projectileScene.Instantiate<Area2D>();
        projectile.GlobalPosition = _muzzle.GlobalPosition;

        Vector2 direction = -_muzzle.GlobalTransform.Y.Normalized();

        if (projectile is Torpedo body)
        {
            body.Direction = direction;
            body.Rotation = direction.Angle() + Mathf.Pi / 2;
        }

        GetTree().Root.AddChild(projectile);
    }
}
