using Godot;

public partial class EnemyShip : Ship
{
    private NavigationAgent2D navAgent;
    private new float Speed = 800;
    private const float DetectionRadius = 2000f;
    private PlayerShip _target;
    private Timer _cannonShotCooldownTimer;

    public override void _Ready()
    {
        navAgent = GetNode<NavigationAgent2D>("EnemyNavigationAgent");
        navAgent.PathDesiredDistance = 4.0f;
        navAgent.TargetDesiredDistance = 4.0f;
        _cannonShotCooldownTimer = new Timer();
        _cannonShotCooldownTimer.WaitTime = 3.0;
        _cannonShotCooldownTimer.OneShot = true;
        AddChild(_cannonShotCooldownTimer);
        _target = ShipManager.Instance.CurrentShip;

        AddFloor(new Vector2I((int)(Position.X - 16), (int)(Position.Y + 16)), FloorTileType.Wood);
        AddFloor(new Vector2I((int)(Position.X - 16), (int)(Position.Y + 48)), FloorTileType.Wood);
        AddFloor(new Vector2I((int)(Position.X - 16), (int)(Position.Y + 80)), FloorTileType.Wood);
        AddFloor(new Vector2I((int)(Position.X - 16), (int)(Position.Y + 112)), FloorTileType.Wood);
        AddFloor(new Vector2I((int)(Position.X + 16), (int)(Position.Y + 16)), FloorTileType.Wood);
        AddFloor(new Vector2I((int)(Position.X + 16), (int)(Position.Y + 48)), FloorTileType.Wood);
        AddFloor(new Vector2I((int)(Position.X + 16), (int)(Position.Y + 80)), FloorTileType.Wood);
        AddFloor(new Vector2I((int)(Position.X + 16), (int)(Position.Y + 112)), FloorTileType.Wood);
        PlaceStructure(new Vector2I((int)(Position.X + 0), (int)(Position.Y + 32)), GunType.Cannon, 0);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null || navAgent == null)
            return;

        float distanceToPlayer = GlobalPosition.DistanceTo(_target.GlobalPosition);

        if (distanceToPlayer <= DetectionRadius)
        {
            navAgent.TargetPosition = _target.GlobalPosition;
            Vector2 nextPoint = navAgent.GetNextPathPosition();

            if (nextPoint != GlobalPosition)
            {
                Vector2 direction = (nextPoint - GlobalPosition).Normalized();
                float targetAngle = direction.Angle() + Mathf.Pi / 2;
                Rotation = Mathf.LerpAngle(Rotation, targetAngle, (float)(delta * 2.0));

                Velocity += -Transform.Y * Speed * (float)delta;
            }

            ShootCannons();
        }

        Velocity = Velocity.Lerp(Vector2.Zero, 0.1f);
        Velocity = Velocity.LimitLength(1000);

        MoveAndSlide();
    }
    protected override void ShootCannons()
    {
        if (_cannonShotCooldownTimer.IsStopped())
        {
            foreach (var structure in StructuresOrigin.Values)
            {
                if (structure is Cannon2x2 cannon)
                {
                    cannon.Shoot();
                }
            }
            _cannonShotCooldownTimer.Start();
        }
    }
}
