using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using System.Collections.Generic;

public partial class PlayerShip : Ship
{

    public PlayerResourceManager playerResourceManager;

    private PackedScene _shipSlotScene;
    private Camera2D _camera;

    private int _minX = int.MaxValue;
    private int _maxX = int.MinValue;
    private int _minY = int.MaxValue;
    private int _maxY = int.MinValue;

    private Vector2 velocity = Vector2.Zero;
    private float rotationSpeed = 0.5f;
    private Timer _shotCooldownTimer;

    public override void _Ready()
    {
        if (playerResourceManager == null)
        {
            playerResourceManager = new PlayerResourceManager();
        }
        _shipSlotScene = GD.Load<PackedScene>("res://Tiles/ShipSlot.tscn");
        _camera = GetNode<Camera2D>("Camera");
        // _camera.Zoom = new Vector2(0.25f, 0.25f);
        playerResourceManager.IncreaseResource(ResourceEnum.Wood, 100);
        playerResourceManager.IncreaseResource(ResourceEnum.Iron, 100);
        playerResourceManager.IncreaseResource(ResourceEnum.Scrap, 100);
        playerResourceManager.IncreaseResource(ResourceEnum.Tridentis, 100);
        _shotCooldownTimer = new Timer
        {
            OneShot = true,
            WaitTime = 1.0f
        };
        AddChild(_shotCooldownTimer);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            switch (keyEvent.Keycode)
            {
                case Key.F:
                    ShootCannons();
                    break;
                case Key.T:
                    ShootTorpedos();
                    break;
                default:
                    break;
            }
        }
    }

    private Vector2 GetCenterWorldPosition()
    {
        if (_minX > _maxX || _minY > _maxY)
        {
            GD.PrintErr("No tiles placed yet. Bounds are invalid.");
            return Vector2.Zero;
        }

        float centerX = (_minX + _maxX) / 2f;
        float centerY = (_minY + _maxY) / 2f;
        return new Vector2(centerX, centerY);
    }

    public void UpdateBounds(Vector2I worldPos)
    {
        var offset = Globals.TILE_SIZE / 2;
        if (worldPos.X - offset < _minX) _minX = worldPos.X - offset;
        if (worldPos.X + offset > _maxX) _maxX = worldPos.X + offset;
        if (worldPos.Y - offset < _minY) _minY = worldPos.Y - offset;
        if (worldPos.Y + offset > _maxY) _maxY = worldPos.Y + offset;
    }

    public void DisableCamera()
    {
        _camera.Enabled = false;
    }

    public void EnableCamera()
    {
        _camera.Enabled = true;
    }

    public void GoOutOfDock()
    {
        Position = ShipManager.Instance.GetSavedShipWorldPosition();
        var centerPos = GetCenterWorldPosition();
        EnableCamera();

        foreach (Node child in GetChildren())
        {
            if (child == _camera) continue;

            if (child is Node2D node2D)
            {
                node2D.Position -= centerPos;
            }
        }
    }

    public void GoToDock()
    {
        Position = Vector2.Zero;
        DisableCamera();
        StopMovement();

        foreach (var (position, (floor, collisionShape)) in Floors)
        {
            floor.Position = position;
            collisionShape.Position = position;
        }
        foreach (var (position, structure) in StructuresOrigin)
        {
            structure.Position = position;
        }
    }

    private void StopMovement()
    {
        velocity = Vector2.Zero;
        Rotation = 0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        if ((!_camera?.Enabled ?? false) || Floors.Count == 0) return;

        float deltaTime = (float)delta;
        Vector2 forward = -Transform.Y;

        if (Input.IsActionPressed("ui_up"))
        {
            velocity += forward * Speed * deltaTime;
        }

        if (Input.IsActionPressed("ui_left"))
        {
            Rotation -= rotationSpeed * deltaTime;
        }

        if (Input.IsActionPressed("ui_right"))
        {
            Rotation += rotationSpeed * deltaTime;
        }

        velocity = velocity.Lerp(Vector2.Zero, 0.1f);
        velocity = velocity.LimitLength(1000);

        Velocity = velocity;
        MoveAndSlide();
    }

    protected override void ShootCannons()
    {

        if (_shotCooldownTimer.IsStopped())
        {
            foreach (var structure in StructuresOrigin.Values)
            {
                if (structure is Cannon2x2 cannon)
                {
                    cannon.Shoot();
                }
            }

            _shotCooldownTimer.Start();
        }
    }

    protected void ShootTorpedos()
    {
        if (_shotCooldownTimer.IsStopped())
        {
            foreach (var structure in StructuresOrigin.Values)
            {
                if (structure is TorpedoLauncher torpedo)
                {
                    torpedo.Shoot();
                }
            }

            _shotCooldownTimer.Start();
        }
    }

    public void ShootRockets(Vector2 target)
    {
        if (_shotCooldownTimer.IsStopped())
        {
            foreach (var structure in StructuresOrigin.Values)
            {
                if (structure is RocketLauncher rocketLauncher)
                {
                    rocketLauncher.Shoot(target);
                }
            }

            _shotCooldownTimer.Start();
        }
    }

    public bool IsPointWithinMiningRange(Vector2 point)
    {
        float width = _maxX - _minX;
        float height = _maxY - _minY;

        Vector2 center = Position;

        float radius = Mathf.Sqrt(width * width + height * height) / 2f;

        float totalRadius = radius + Globals.MAX_MINING_DISTANCE;

        float distance = center.DistanceTo(point);

        return distance <= totalRadius;

    }
}
