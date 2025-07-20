using Godot;
using System;

public partial class ShipSlot : Node2D
{
    public FloorTile floorTile;
    public Node2D gun;

    public void Init(Vector2I position)
    {
        GlobalPosition = new Vector2(position.X, position.Y);
    }

    public void SetFloor(FloorTile floor)
    {
        if (floorTile != null)
            floorTile.QueueFree();

        floorTile = floor;
        AddChild(floorTile);
    }

}
