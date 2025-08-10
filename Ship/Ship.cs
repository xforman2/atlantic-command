using Godot;
using System;
using System.Collections.Generic;

public abstract partial class Ship : CharacterBody2D
{
    public Dictionary<Vector2I, (FloorTile, CollisionShape2D)> Floors = new();
    public Dictionary<Vector2I, BuildableStructure> Structures = new();
    public Dictionary<Vector2I, BuildableStructure> StructuresOrigin = new();
    public event Action ShipDestroyed;

    [Export]
    public int Speed { get; set; } = 1000;

    private PackedScene _shipSlotScene;

    private float rotationSpeed = 0.5f;

    public void AddFloor(Vector2I position, FloorTileType type)
    {
        FloorTile floor = Globals.tileScenes[type].Instantiate<FloorTile>();
        floor.Init(position);
        if (Floors.ContainsKey(position))
        {
            GD.PrintErr("Floor is already present on: ", position);
            return;
        }
        AddChild(floor);
        var collisionShape = AddCollisionShapeForTile(position, floor);
        Floors[position] = (floor, collisionShape);
    }

    public void ToggleStructuresVisibility(bool visible)
    {
        foreach (var structure in StructuresOrigin.Values)
            structure.Visible = visible;
    }


    public void ToggleHpLabelsVisibility(bool visible)
    {
        foreach (var floorData in Floors.Values)
            floorData.Item1.ShowHpLabel(visible);
    }

    private CollisionShape2D AddCollisionShapeForTile(Vector2I position, FloorTile floor)
    {
        var collisionShape = new CollisionShape2D();
        var rectShape = new RectangleShape2D
        {
            Size = new Vector2(Globals.TILE_SIZE, Globals.TILE_SIZE)
        };
        collisionShape.Shape = rectShape;
        collisionShape.Position = position;
        collisionShape.SetMeta("floor_tile", floor);
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

    protected abstract void ShootCannons();

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

    public void PlaceStructure(Vector2I worldPos, GunType type, float rotationDegrees)
    {

        var occupiedPositions = Globals.GetOccupiedPositions(worldPos, Globals.gunSizes[type]);
        BuildableStructure structure = Globals.gunScenes[type].Instantiate<BuildableStructure>();
        structure.Init(worldPos, occupiedPositions, rotationDegrees);
        foreach (var occupiedPos in structure.OccupiedPositions)
        {
            Structures[occupiedPos] = structure;
            if (Floors.TryGetValue(occupiedPos, out var floorData))
            {
                FloorTile floor = floorData.Item1;
                floor.StructureOnTop = structure;
            }
        }
        StructuresOrigin[structure.Origin] = structure;
        AddChild(structure);
    }
    public bool IsDestroyed()
    {
        return Floors.Count == 0;
    }

    public virtual void DropResources()
    {
    }

    public void OnShipDestroyed()
    {
        ShipDestroyed?.Invoke();
    }

}
