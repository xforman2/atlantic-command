using Godot;
using System;
using System.Collections.Generic;

public partial class Globals : Node
{
    public static readonly Dictionary<FloorTileType, PackedScene> tileScenes = new()
    {
        { FloorTileType.Wood, GD.Load<PackedScene>("res://Tiles/WoodFloorTile.tscn") },
        { FloorTileType.Iron, GD.Load<PackedScene>("res://Tiles/IronFloorTile.tscn") },
        { FloorTileType.Steel, GD.Load<PackedScene>("res://Tiles/SteelFloorTile.tscn") }
    };


    public static readonly Dictionary<GunType, PackedScene> gunScenes = new()
    {
        { GunType.Cannon, GD.Load<PackedScene>("res://Guns/Cannon2x2.tscn") },
        { GunType.RocketLauncher, GD.Load<PackedScene>("res://Guns/RocketLauncher.tscn") }
    };

    public static readonly Dictionary<GunType, Vector2I> gunSizes = new() {
        { GunType.Cannon, new Vector2I(64, 64) },
        { GunType.RocketLauncher, new Vector2I(64, 64) }
    };


    public static readonly Dictionary<FloorTileType, Dictionary<ResourceEnum, int>> tileCosts = new()
    {
        { FloorTileType.Wood, new Dictionary<ResourceEnum, int> { { ResourceEnum.Wood, 4 } } },
        { FloorTileType.Iron, new Dictionary<ResourceEnum, int> { { ResourceEnum.Iron, 6 }, { ResourceEnum.Wood, 2 } } },
        { FloorTileType.Steel, new Dictionary<ResourceEnum, int> { { ResourceEnum.Iron, 6 }, { ResourceEnum.Wood, 2 } } }
    };

    public static int TILE_SIZE = 32;
    public static int CHUNK_SIZE = 32;
    public static int MAX_MINING_DISTANCE = 5 * TILE_SIZE;
    public static Vector2 STARTING_POSITION = new Vector2(1080, 190);

    public static List<Vector2I> GetOccupiedPositions(Vector2I center, Vector2I size)
    {
        var result = new List<Vector2I>();

        int tilesWide = size.X / Globals.TILE_SIZE;
        int tilesHigh = size.Y / Globals.TILE_SIZE;

        int halfWidth = size.X / 2;
        int halfHeight = size.Y / 2;

        for (int x = 0; x < tilesWide; x++)
        {
            for (int y = 0; y < tilesHigh; y++)
            {
                int offsetX = x * Globals.TILE_SIZE - halfWidth + (tilesWide % 2 == 0 ? Globals.TILE_SIZE / 2 : 0);
                int offsetY = y * Globals.TILE_SIZE - halfHeight + (tilesHigh % 2 == 0 ? Globals.TILE_SIZE / 2 : 0);

                result.Add(center + new Vector2I(offsetX, offsetY));
            }
        }

        return result;
    }
}
