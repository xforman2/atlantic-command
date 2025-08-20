using Godot;
using System;

public partial class SteelFloorTile : FloorTile
{
    protected override int DefaultHP => 200;
    public override FloorTileType TileType => FloorTileType.Steel;
}
