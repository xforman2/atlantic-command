using Godot;
using System.Collections.Generic;

public partial class Ship : RigidBody2D
{
    public Dictionary<Vector2I, FloorTile> Floors = new();
    public Dictionary<Vector2I, BuildableStructure> Structures = new();

    [Export]
    public int Speed { get; set; } = 300;

    public PlayerResourceManager playerResourceManager;

    private PackedScene _shipSlotScene;
    private Camera2D _camera;

    private int _minX = int.MaxValue;
    private int _maxX = int.MinValue;
    private int _minY = int.MaxValue;
    private int _maxY = int.MinValue;

    public override void _Ready()
    {
        if (playerResourceManager == null)
        {
            playerResourceManager = new PlayerResourceManager();
        }
        _shipSlotScene = GD.Load<PackedScene>("res://Tiles/ShipSlot.tscn");
        _camera = GetNode<Camera2D>("Camera");
        playerResourceManager.IncreaseCoal(100);
        playerResourceManager.IncreaseWood(100);
        playerResourceManager.IncreaseCopper(100);
        playerResourceManager.IncreaseIron(100);
        this.BodyEntered += OnBodyEntered;
        Inertia = 1;
        GravityScale = 0;
        LinearDamp = 2f;
        AngularDamp = 2f;

    }

    private void OnBodyEntered(Node body)
    {
        GD.Print("Collided with: " + body.Name);
    }

    public void AddFloor(Vector2I position, FloorTile floor)
    {
        if (Floors.ContainsKey(position))
        {
            GD.Print("Floor is already present on: ", position);
            return;
        }
        Floors[position] = floor;
        AddChild(floor);

        AddCollisionShapeForTile(position, floor);
    }

    private void AddCollisionShapeForTile(Vector2I position, FloorTile floor)
    {
        var collisionShape = new CollisionShape2D();
        var rectShape = new RectangleShape2D();
        rectShape.Size = new Vector2(Globals.TILE_SIZE, Globals.TILE_SIZE);
        collisionShape.Shape = rectShape;
        collisionShape.Position = position;


        AddChild(collisionShape);
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

        foreach (var offset in neighbors)
        {
            var neighborPos = position + offset;
            if (Floors.ContainsKey(neighborPos))
                return true;
        }

        return false;
    }

    public bool CanPlaceStructure(GunType gunType, Vector2I position)
    {
        var offsets = GetOccupiedTileOffsets(gunType);
        foreach (var offset in offsets)
        {
            var occupiedPos = position + offset;
            if (!Floors.ContainsKey(occupiedPos) || Structures.ContainsKey(occupiedPos))
            {
                return false;
            }
        }
        return true;
    }

    public void PlaceStructure(BuildableStructure structure)
    {
        Structures[structure.Origin] = structure;
        AddChild(structure);
    }

    private List<Vector2I> GetOccupiedTileOffsets(GunType gunType)
    {
        return gunType switch
        {
            GunType.Cannon => new List<Vector2I>
            {
                new Vector2I(0, 0),
                new Vector2I(Globals.TILE_SIZE, 0),
                new Vector2I(0, Globals.TILE_SIZE),
                new Vector2I(Globals.TILE_SIZE, Globals.TILE_SIZE)
            },
            _ => new List<Vector2I> { new Vector2I(0, 0) }
        };
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

        foreach (var (position, floor) in Floors)
        {
            floor.Position = position;
        }
        foreach (var (position, structure) in Structures)
        {
            structure.Position = position;
        }
    }

    private void StopMovement()
    {
        LinearVelocity = Vector2.Zero;
        AngularVelocity = 0f;
        Rotation = 0f;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_camera?.Enabled ?? false) return;

        Vector2 forward = -Transform.Y;

        if (Input.IsActionPressed("ui_up"))
        {
            ApplyCentralForce(forward * Speed);
        }

        if (Input.IsActionPressed("ui_left"))
        {
            ApplyTorque(-3f);
        }

        if (Input.IsActionPressed("ui_right"))
        {
            ApplyTorque(3f);
        }
    }

}
