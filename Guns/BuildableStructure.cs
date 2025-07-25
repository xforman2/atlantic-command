using Godot;
using System;

public abstract partial class BuildableStructure : Node2D
{
    public Vector2I Origin;
    public Vector2I[] OccupiedSlots;

    public abstract void Init(Vector2I origin, float rotation);
}
