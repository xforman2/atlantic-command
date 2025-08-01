using Godot;
using System.Collections.Generic;

public partial class Ship : CharacterBody2D
{
    public Dictionary<Vector2I, (FloorTile, CollisionShape2D)> Floors = new();
    public Dictionary<Vector2I, BuildableStructure> Structures = new();
    public Dictionary<Vector2I, BuildableStructure> StructuresOrigin = new();

    [Export]
    public int Speed { get; set; } = 1000;

    public PlayerResourceManager playerResourceManager;

    private PackedScene _shipSlotScene;
    private Camera2D _camera;

    private int _minX = int.MaxValue;
    private int _maxX = int.MinValue;
    private int _minY = int.MaxValue;
    private int _maxY = int.MinValue;

    private Vector2 velocity = Vector2.Zero;
    private float rotationSpeed = 0.5f;
    private Timer _cannonShotCooldownTimer;

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
        _cannonShotCooldownTimer = new Timer
        {
            OneShot = true,
            WaitTime = 1.0f
        };
        AddChild(_cannonShotCooldownTimer);
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

                default:
                    break;
            }
        }
    }

    public void AddFloor(Vector2I position, FloorTile floor)
    {
        if (Floors.ContainsKey(position))
        {
            GD.PrintErr("Floor is already present on: ", position);
            return;
        }
        AddChild(floor);
        var collisionShape = AddCollisionShapeForTile(position, floor);
        Floors[position] = (floor, collisionShape);
    }

    private CollisionShape2D AddCollisionShapeForTile(Vector2I position, FloorTile floor)
    {
        var collisionShape = new CollisionShape2D();
        var rectShape = new RectangleShape2D();
        rectShape.Size = new Vector2(Globals.TILE_SIZE, Globals.TILE_SIZE);
        collisionShape.Shape = rectShape;
        collisionShape.Position = position;

        AddChild(collisionShape);
        return collisionShape;
    }

    public bool CanAddFloor(Vector2I position)
    {
        if (Floors.Count == 0)
            return true;
        if (Floors.ContainsKey(position))
            return false;

        Vector2I[] neighbors = new Vector2I[]
        {
            new Vector2I(0, -Globals.TILE_SIZE),
            new Vector2I(0, Globals.TILE_SIZE),
            new Vector2I(-Globals.TILE_SIZE, 0),
            new Vector2I(Globals.TILE_SIZE, 0)
        };

        bool buildable = false;
        foreach (var offset in neighbors)
        {
            var neighborPos = position + offset;
            if (Floors.ContainsKey(neighborPos))
                buildable = true;
            if (Structures.ContainsKey(neighborPos))
            {
                BuildableStructure structure = Structures[neighborPos];
                if (structure is Cannon2x2 cannon)
                {
                    var direction = cannon.GetRotationDirection();
                    var frontPositions = cannon.GetFrontPositions();
                    if (frontPositions.Contains(neighborPos))
                    {
                        if (neighborPos + direction * Globals.TILE_SIZE == position)
                        {
                            return false;
                        }
                    }
                }
            }
        }

        return buildable;
    }

    private void ShootCannons()
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

    public bool CanPlaceStructure(List<Vector2I> occupiedPositions)
    {
        foreach (var position in occupiedPositions)
        {
            if (!Floors.ContainsKey(position) || Structures.ContainsKey(position))
            {
                return false;
            }
        }
        return true;
    }

    public void PlaceStructure(BuildableStructure structure)
    {
        foreach (var occupiedPos in structure.OccupiedPositions)
        {
            Structures[occupiedPos] = structure;
        }
        StructuresOrigin[structure.Origin] = structure;
        AddChild(structure);
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

    public void UpdateBounds(Vector2I worldPos, int tileSize)
    {
        if (worldPos.X < _minX) _minX = worldPos.X;
        if ((worldPos.X + tileSize) > _maxX) _maxX = worldPos.X + tileSize;
        if (worldPos.Y < _minY) _minY = worldPos.Y;
        if ((worldPos.Y + tileSize) > _maxY) _maxY = worldPos.Y + tileSize;
    }

    public void GoOutOfDock()
    {
        GlobalPosition = GetCenterWorldPosition();
        _camera.Enabled = true;

        foreach (Node child in GetChildren())
        {
            if (child == _camera) continue;

            if (child is Node2D node2D)
            {
                node2D.Position -= Position;
            }
        }
    }

    public void GoToDock()
    {
        GlobalPosition = Vector2.Zero;
        _camera.Enabled = false;
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
}
