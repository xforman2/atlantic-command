using Godot;
using System;
using System.Collections.Generic;

public partial class EnemySpawner : Node
{
	[Export] public PackedScene EnemyScene;
	[Export] public float SpawnChancePerTile = 0.0002f;

	private List<Node2D> _activeEnemies = new();

	public void SpawnEnemiesInChunk(Vector2I chunkPos, TileMapLayer groundLayer)
	{
		if (EnemyScene == null || groundLayer == null)
			return;

		var rng = new Random(chunkPos.GetHashCode());
		Vector2I start = chunkPos * Globals.CHUNK_SIZE;

		for (int x = 0; x < Globals.CHUNK_SIZE - 5; x++)
		{
			for (int y = 0; y < Globals.CHUNK_SIZE - 1; y++)
			{
				if (rng.NextDouble() < SpawnChancePerTile)
				{

					Vector2I topLeft = start + new Vector2I(x, y);

					if (IsWaterAreaAvailable(topLeft, groundLayer))
					{
						Vector2 spawnPos = groundLayer.MapToLocal(topLeft) + new Vector2(Globals.TILE_SIZE * 3, Globals.TILE_SIZE);
						var enemy = EnemyScene.Instantiate<EnemyShip>();

						enemy.Position = spawnPos;
						groundLayer.GetParent().AddChild(enemy);
						_activeEnemies.Add(enemy);
					}
				}
			}
		}
	}

	public void DespawnEnemiesInChunk(Vector2I chunkPos, TileMapLayer groundLayer)
	{
		Vector2I start = chunkPos * Globals.CHUNK_SIZE;
		Rect2 chunkBounds = new Rect2(
			groundLayer.MapToLocal(start),
			new Vector2(Globals.CHUNK_SIZE * Globals.TILE_SIZE, Globals.CHUNK_SIZE * Globals.TILE_SIZE)
		);

		var toRemove = new List<Node2D>();
		foreach (var enemy in _activeEnemies)
		{
			if (chunkBounds.HasPoint(enemy.Position))
			{
				enemy.QueueFree();
				toRemove.Add(enemy);
			}
		}

		foreach (var e in toRemove)
			_activeEnemies.Remove(e);
	}

	private bool IsWaterAreaAvailable(Vector2I topLeft, TileMapLayer groundLayer)
	{
		for (int dx = 0; dx < 6; dx++)
		{
			for (int dy = 0; dy < 2; dy++)
			{
				Vector2I tilePos = topLeft + new Vector2I(dx, dy);
				int tileId = groundLayer.GetCellSourceId(tilePos);

				if (tileId != (int)GroundTextureEnum.Water)
					return false;
			}
		}

		return true;
	}
}
