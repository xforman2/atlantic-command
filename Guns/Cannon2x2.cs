using System.Collections.Generic;
using Godot;

public partial class Cannon2x2 : Gun
{
    private PackedScene _projectileScene;
    private Marker2D _muzzle;
    private AudioStreamPlayer2D _shootSound;

    public override void _Ready()
    {
        _projectileScene = GD.Load<PackedScene>("res://Projectiles/CannonBall.tscn");
        _muzzle = GetNode<Marker2D>("Muzzle");
        _shootSound = new AudioStreamPlayer2D();
        _shootSound.Stream = GD.Load<AudioStream>("res://Audio/cannon.mp3");
        _shootSound.Bus = "SFX";
        AddChild(_shootSound);
    }

    public override void _Input(InputEvent @event)
    {


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

        if (projectile is CannonBall body)
        {
            body.Direction = direction;
        }

        GetTree().Root.AddChild(projectile);
        _shootSound?.Play();
    }

}
