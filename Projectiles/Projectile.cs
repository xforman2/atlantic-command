using Godot;
using System;
using System.Linq;

public abstract partial class Projectile : Area2D
{

    protected virtual int DefaultDamage => 100;
    private int Damage;
    private bool _hasHit = false;
    private EnemySpawner _enemySpawner;

    public override void _Ready()
    {
        _enemySpawner = GetTree().Root.GetNode<EnemySpawner>("Game/EnemySpawner");
        Damage = DefaultDamage;
    }

    protected void OnBodyShapeEntered(Rid bodyRid, Node2D body, long bodyShapeIndex, long localShapeIndex)
    {
        if (_hasHit) return;
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
        _hasHit = true;
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

        if (ship.IsDestroyed())
        {
            ship.DropResources();
            ship.OnShipDestroyed();
            if (ship is EnemyShip enemyShip)
            {
                _enemySpawner.ActiveEnemies.Remove(enemyShip);
                enemyShip.QueueFree();
            }
        }
    }
}
