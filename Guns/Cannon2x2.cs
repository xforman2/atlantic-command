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

    public override void Init(Vector2I origin)
    {
        Origin = origin;

        OccupiedSlots = new Vector2I[]
        {
            origin,
            origin + new Vector2I(Globals.TILE_SIZE, 0),
            origin + new Vector2I(0, Globals.TILE_SIZE),
            origin + new Vector2I(Globals.TILE_SIZE, Globals.TILE_SIZE)
        };

        Position = origin;

        GD.Print("Pos:", Position);
        GD.Print("GP:", GlobalPosition);
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

        var direction = -GlobalTransform.Y.Normalized();
        if (projectile is RigidBody2D body)
        {
            body.ApplyImpulse(Vector2.Zero, direction * 300);
        }

        GetTree().Root.AddChild(projectile);
    }
}
