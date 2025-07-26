using Godot;
using System;

public abstract partial class FloorTile : Node2D
{

    public void Init(Vector2I position)
    {
        Position = position;
    }

}
