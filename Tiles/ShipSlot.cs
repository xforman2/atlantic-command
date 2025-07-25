using Godot;
using System;

public partial class ShipSlot : Node2D
{
    public FloorTile floorTile;
    public BuildableStructure buildableStructure;

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
    public void SetStructure(BuildableStructure structure)
    {
        buildableStructure = structure;
    }

    public void RemoveStructure()
    {
        if (buildableStructure != null)
        {
            buildableStructure.QueueFree();
            buildableStructure = null;
        }
    }

}
