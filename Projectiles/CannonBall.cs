using Godot;
using System;
using System.Linq;

public partial class CannonBall : Area2D
{


    [Export] public int Damage { get; set; } = 25;
    [Export] public float Speed { get; set; } = 300f;

    public Vector2 Direction { get; set; }

    public override void _Ready()
    {
        GetTree().CreateTimer(5).Timeout += QueueFree;
        BodyShapeEntered += OnBodyShapeEntered;
    }
    public override void _PhysicsProcess(double delta)
    {
        Position += Direction * Speed * (float)delta;
    }

    private void OnBodyShapeEntered(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
    {
        if (body is Ship ship)
        {
            var shapes = ship.GetChildren().OfType<CollisionShape2D>().ToArray();

            if (bodyShapeIndex >= 0 && bodyShapeIndex < shapes.Length)
            {
                var hitShape = shapes[bodyShapeIndex];
                FloorTile tile = hitShape.GetMeta("floor_tile").As<FloorTile>();

                if (tile != null)
                {
                    tile.TakeDamage(Damage);

                    if (tile.HP <= 0)
                    {
                        DestroyTile(ship, tile, hitShape);
                    }
                }
            }
        }
        QueueFree();
    }

    private void DestroyTile(Ship ship, FloorTile tile, CollisionShape2D hitShape)
    {
        ship.Floors.Remove(tile.Origin);

        if (tile.StructureOnTop != null)
        {
            foreach (var structurePos in tile.StructureOnTop.OccupiedPositions)
            {
                if (ship.Floors.TryGetValue(structurePos, out var floorTuple))
                    floorTuple.Item1.StructureOnTop = null;

                ship.Structures.Remove(structurePos);
            }

            ship.StructuresOrigin.Remove(tile.StructureOnTop.Origin);

            tile.StructureOnTop.QueueFree();
            tile.StructureOnTop = null;
        }

        hitShape.QueueFree();
    }
}
