using Godot;

public partial class EnemyShip : Ship
{
    private NavigationAgent2D navAgent;
    private const float MoveSpeed = 100f;
    private const float DetectionRadius = 2000f;
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

        AddFloor(new Vector2I((int)(Position.X + 0), (int)(Position.Y + 0)), FloorTileType.Wood);
        AddFloor(new Vector2I((int)(Position.X + 0), (int)(Position.Y + 32)), FloorTileType.Wood);
        AddFloor(new Vector2I((int)(Position.X + 0), (int)(Position.Y + 64)), FloorTileType.Wood);
        AddFloor(new Vector2I((int)(Position.X + 0), (int)(Position.Y + 96)), FloorTileType.Wood);
        AddFloor(new Vector2I((int)(Position.X + 32), (int)(Position.Y + 0)), FloorTileType.Wood);
        AddFloor(new Vector2I((int)(Position.X + 32), (int)(Position.Y + 32)), FloorTileType.Wood);
        AddFloor(new Vector2I((int)(Position.X + 32), (int)(Position.Y + 64)), FloorTileType.Wood);
        AddFloor(new Vector2I((int)(Position.X + 32), (int)(Position.Y + 96)), FloorTileType.Wood);
        PlaceStructure(new Vector2I((int)(Position.X + 16), (int)(Position.Y + 16)), GunType.Cannon, 0);
    }

    public override void _PhysicsProcess(double delta)
    {
        var player = ShipManager.Instance.CurrentShip;
        if (player == null || navAgent == null)
            return;

        float distanceToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);

        if (distanceToPlayer <= DetectionRadius)
        {
            navAgent.TargetPosition = player.GlobalPosition;
            Vector2 nextPoint = navAgent.GetNextPathPosition();

            if (nextPoint != GlobalPosition)
            {
                Vector2 direction = (nextPoint - GlobalPosition).Normalized();
                float targetAngle = direction.Angle() + Mathf.Pi / 2;
                Rotation = Mathf.LerpAngle(Rotation, targetAngle, (float)(delta * 2.0));
                Velocity = -Transform.Y * MoveSpeed;
                MoveAndSlide();
            }

            ShootCannons();
        }
        else
        {
            Velocity = Vector2.Zero;
            MoveAndSlide();
        }
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
