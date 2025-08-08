using Godot;
using System;
using System.Collections.Generic;

public partial class RocketLauncher : Gun
{
    private PackedScene _projectileScene;
    private Marker2D _muzzle;

    public override void _Ready()
    {
        _projectileScene = GD.Load<PackedScene>("res://Projectiles/Rocket.tscn");
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
        if (_projectileScene == null || _muzzle == null || target == null)
        {
            GD.PrintErr("Missing projectile, muzzle, or target.");
            return;
        }

        var projectile = _projectileScene.Instantiate<Area2D>();
        projectile.GlobalPosition = _muzzle.GlobalPosition;

        Vector2 targetPos = target.Value;
        Vector2 direction = (targetPos - projectile.GlobalPosition).Normalized();

        if (projectile is Rocket body)
        {
            body.Direction = direction;
            body.Rotation = direction.Angle() + Mathf.Pi / 2;
        }

        GetTree().Root.AddChild(projectile);
    }
}
