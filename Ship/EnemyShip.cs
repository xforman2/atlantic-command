using Godot;
using System;

public abstract partial class EnemyShip : Ship
{
    protected NavigationAgent2D navAgent;
    protected new float Speed = 800f;
    protected const float DetectionRadius = 1000f;
    protected PlayerShip _target;
    protected Timer _cannonShotCooldownTimer;

    public override void _Ready()
    {
        navAgent = GetNode<NavigationAgent2D>("EnemyNavigationAgent");
        _cannonShotCooldownTimer = new Timer();
        _cannonShotCooldownTimer.WaitTime = 3.0;
        _cannonShotCooldownTimer.OneShot = true;
        AddChild(_cannonShotCooldownTimer);

        _target = ShipManager.Instance.CurrentShip;

        InitializeShipSpecifics();
    }

    protected abstract void InitializeShipSpecifics();

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null || navAgent == null || _target.IsDestroyed())
            return;

        float distanceToPlayer = GlobalPosition.DistanceTo(_target.GlobalPosition);

        if (distanceToPlayer <= DetectionRadius)
        {
            navAgent.TargetPosition = _target.GlobalPosition;
            Vector2 nextPoint = navAgent.GetNextPathPosition();

            if (!navAgent.IsNavigationFinished())
            {
                Vector2 direction = (nextPoint - GlobalPosition).Normalized();
                float targetAngle = direction.Angle() + Mathf.Pi / 2;
                Rotation = Mathf.LerpAngle(Rotation, targetAngle, (float)(delta * 2.0));
                Velocity += -Transform.Y * Speed * (float)delta;
            }

            ShootCannons();
            ShootTorpedos();
        }

        Velocity = Velocity.Lerp(Vector2.Zero, 0.1f);
        Velocity = Velocity.LimitLength(1000);

        MoveAndSlide();
    }

    public abstract override void DropResources();

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
        }
    }

    protected void ShootTorpedos()
    {
        if (_cannonShotCooldownTimer.IsStopped())
        {
            foreach (var structure in StructuresOrigin.Values)
            {
                if (structure is TorpedoLauncher torpedoLauncher)
                {
                    torpedoLauncher.Shoot();
                }
            }
            _cannonShotCooldownTimer.Start();
        }
    }
}
