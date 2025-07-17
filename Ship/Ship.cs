using Godot;
using System.Collections.Generic;

public partial class Ship : Node2D
{
	public Dictionary<Vector2I, FloorTile> Tiles = new();

	public PlayerResourceManager _resourceManager;

	public override void _Ready()
	{
		_resourceManager = new PlayerResourceManager();
	}

	public void AddTile(FloorTile tile, Vector2I gridPos)
	{
		tile.Init(gridPos);
		AddChild(tile);
		Tiles[gridPos] = tile;
	}

	private Vector2 GridToWorld(Vector2I gridPos)
	{
		const int TILE_SIZE = 128;
		return new Vector2(gridPos.X * TILE_SIZE, gridPos.Y * TILE_SIZE);
	}
}
