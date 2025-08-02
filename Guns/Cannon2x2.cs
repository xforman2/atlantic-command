using System.Collections.Generic;
using Godot;

public partial class Cannon2x2 : Gun
{
    private PackedScene _projectileScene;
    private Marker2D _muzzle;
    private Timer _fireTimer;

    public override void _Ready()
    {
        _projectileScene = GD.Load<PackedScene>("res://Projectiles/CannonBall.tscn");
        _muzzle = GetNode<Marker2D>("Muzzle");
        _fireTimer = new Timer
        {
            WaitTime = 2.0,    // seconds between shots
            OneShot = false,
            Autostart = true
        };
        AddChild(_fireTimer);
        _fireTimer.Timeout += Shoot;
        _fireTimer.Start();
    }

    public override void Init(Vector2I origin, List<Vector2I> occupiedPositions, float rotation)
    {
        Origin = origin;

        OccupiedPositions = occupiedPositions;

        Position = origin;
        RotationDegrees = rotation;
    }

    public override void Shoot()
    {
        if (_projectileScene == null || _muzzle == null)
        {
            GD.PrintErr("Missing projectile or muzzle.");
            return;
        }

        var projectile = _projectileScene.Instantiate<RigidBody2D>();
        projectile.GlobalPosition = _muzzle.GlobalPosition;

        Vector2 direction = -_muzzle.GlobalTransform.Y.Normalized();

        if (projectile is RigidBody2D body)
        {
            body.LinearVelocity = direction * 300;
        }

        GetTree().Root.AddChild(projectile);
    }

}
