using Godot;
using System;

public partial class IronFloorTile : FloorTile
{
    protected override int DefaultHP => 100;
    public override FloorTileType TileType => FloorTileType.Iron;
}
