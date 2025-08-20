using Godot;
using System;

public partial class SteelFloorTile : FloorTile
{
    protected override int DefaultHP => 150;
    public override FloorTileType TileType => FloorTileType.Steel;
}
