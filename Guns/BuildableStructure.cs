using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract partial class BuildableStructure : Node2D
{
    public Vector2I Origin;
    public List<Vector2I> OccupiedPositions;

    public abstract void Init(Vector2I origin, List<Vector2I> occupiedPositions, float rotation);

    public List<Vector2I> GetFrontPositions()
    {

        return ((int)RotationDegrees % 360) switch
        {
            0 => OccupiedPositions.Where(pos => pos.Y == OccupiedPositions.Min(p => p.Y)).ToList(),
            90 => OccupiedPositions.Where(pos => pos.X == OccupiedPositions.Max(p => p.X)).ToList(),
            180 => OccupiedPositions.Where(pos => pos.Y == OccupiedPositions.Max(p => p.Y)).ToList(),
            270 => OccupiedPositions.Where(pos => pos.X == OccupiedPositions.Min(p => p.X)).ToList(),
            _ => new List<Vector2I>()
        };
    }
    public Vector2I GetRotationDirection()
    {

        return ((int)RotationDegrees % 360) switch
        {
            0 => new Vector2I(0, -1),
            90 => new Vector2I(1, 0),
            180 => new Vector2I(0, 1),
            270 => new Vector2I(-1, 0),
            _ => Vector2I.Zero
        };
    }
}
