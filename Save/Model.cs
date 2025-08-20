using System;
using System.Collections.Generic;
using Godot;

[Serializable]
public class ShipSaveData
{
    public Vector2Save Position { get; set; }
    public float RotationDegrees { get; set; }

    public int Wood { get; set; }
    public int Scrap { get; set; }
    public int Iron { get; set; }
    public int Tridentis { get; set; }

    public List<FloorSaveData> Floors { get; set; } = new();
    public List<StructureSaveData> Structures { get; set; } = new();
}

[Serializable]
public class FloorSaveData
{
    public Vector2Save Position { get; set; }
    public FloorTileType Type { get; set; }
    public int HP { get; set; }
}

[Serializable]
public class StructureSaveData
{
    public Vector2Save Origin { get; set; }
    public List<Vector2Save> OccupiedPositions { get; set; } = new();
    public GunType Type { get; set; }
    public float RotationDegrees { get; set; }
}

[Serializable]
public class Vector2Save
{
    public float X { get; set; }
    public float Y { get; set; }

    public Vector2Save() { }
    public Vector2Save(Vector2 vec)
    {
        X = vec.X;
        Y = vec.Y;
    }

    public Vector2 ToVector2() => new Vector2(X, Y);

    public Vector2I ToVector2I() => new Vector2I((int)Math.Floor(X), (int)Math.Floor(Y));
}
