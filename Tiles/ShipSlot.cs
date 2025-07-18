using Godot;
using System;

public partial class ShipSlot : Node2D
{
    private FloorTile floorTile;
    private Node2D buildable;

    public void Init(Vector2I position)
    {
        Position = new Vector2(position.X, position.Y);
    }

    public void SetFloor(FloorTile floor)
    {
        if (floorTile != null)
            floorTile.QueueFree();

        floorTile = floor;
        AddChild(floorTile);
    }

}
