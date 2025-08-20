using Godot;
using System;

public partial class WoodFloorTile : FloorTile
{
    protected override int DefaultHP => 50;
    public override FloorTileType TileType => FloorTileType.Wood;
}
