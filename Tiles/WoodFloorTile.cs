using Godot;
using System;

public partial class WoodFloorTile : FloorTile
{
    public virtual bool CanBePlacedAt(Vector2I gridPos, Ship ship)
    {
        return !ship.Slots.ContainsKey(gridPos);
    }
}
