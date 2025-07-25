using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Game : Node2D
{
    private Ship _ship;
    private TileMapLayer _tileMapLayer;

    [Export] public int ChunkSize = 32;
    [Export] public int ActiveChunkRadius = 2;
    [Export] public float NoiseScale = 0.05f;
    [Export] public float SandThreshold = 0.1f;
    [Export] public NoiseTexture2D NoiseTexture2D;

    private Dictionary<Vector2I, bool> _loadedChunks = new();
    private Vector2I _lastChunkPos = new(-9999, -9999);

    public override void _Ready()
    {
        _tileMapLayer = GetNode<TileMapLayer>("WorldTileMapLayer");

        _ship = ShipManager.Instance.CurrentShip;
        if (_ship == null)
        {
            var scene = GD.Load<PackedScene>("Ship/Ship.tscn");
            _ship = scene.Instantiate<Ship>();
            AddChild(_ship);
            ShipManager.Instance.SetShip(_ship);
            GD.Print("New ship instantiated and assigned to ShipManager.");
        }
        else
        {
            if (_ship.GetParent() != this)
            {
                _ship.Reparent(this);
            }
            GD.Print("Existing ship loaded from ShipManager.");
        }

        _ship.Position = GetViewportRect().GetCenter();
        GD.Print($"Ship position {_ship.Position}");
    }

    public override void _Process(double delta)
    {
        if (_ship == null || _tileMapLayer == null || NoiseTexture2D == null)
            return;

        Vector2I tileSize = _tileMapLayer.TileSet.TileSize;
        Vector2I currentChunk = new Vector2I(
            Mathf.FloorToInt(_ship.Position.X / (ChunkSize * tileSize.X)),
            Mathf.FloorToInt(_ship.Position.Y / (ChunkSize * tileSize.Y))
        );

        if (currentChunk != _lastChunkPos)
        {
            UpdateVisibleChunks(currentChunk);
            _lastChunkPos = currentChunk;
        }
    }

    private void UpdateVisibleChunks(Vector2I centerChunk)
    {
        HashSet<Vector2I> neededChunks = new();

        for (int dx = -ActiveChunkRadius; dx <= ActiveChunkRadius; dx++)
        {
            for (int dy = -ActiveChunkRadius; dy <= ActiveChunkRadius; dy++)
            {
                Vector2I chunkPos = new(centerChunk.X + dx, centerChunk.Y + dy);
                neededChunks.Add(chunkPos);

                if (!_loadedChunks.ContainsKey(chunkPos))
                {
                    GenerateChunk(chunkPos);
                    _loadedChunks[chunkPos] = true;
                }
            }
        }

        var toRemove = _loadedChunks.Keys.Where(c => !neededChunks.Contains(c)).ToList();
        foreach (var chunk in toRemove)
        {
            UnloadChunk(chunk);
            _loadedChunks.Remove(chunk);
        }
    }

    private void GenerateChunk(Vector2I chunkPos)
    {
        var noise = NoiseTexture2D.Noise;
        Vector2I start = chunkPos * ChunkSize;

        for (int x = 0; x < ChunkSize; x++)
        {
            for (int y = 0; y < ChunkSize; y++)
            {
                Vector2I tilePos = start + new Vector2I(x, y);
                float noiseVal = noise.GetNoise2D(tilePos.X * NoiseScale, tilePos.Y * NoiseScale);
                int sourceId = noiseVal > SandThreshold ? 1 : 2;
                _tileMapLayer.SetCell(tilePos, sourceId, Vector2I.Zero);
            }
        }

        GD.Print($"[LOAD] Chunk {chunkPos}");
    }

    private void UnloadChunk(Vector2I chunkPos)
    {
        Vector2I start = chunkPos * ChunkSize;

        for (int x = 0; x < ChunkSize; x++)
        {
            for (int y = 0; y < ChunkSize; y++)
            {
                Vector2I tilePos = start + new Vector2I(x, y);
                _tileMapLayer.SetCell(tilePos, -1, Vector2I.Zero);
            }
        }

        GD.Print($"[UNLOAD] Chunk {chunkPos}");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.B && keyEvent.Pressed && !keyEvent.Echo)
            {
                ShipManager.Instance.SetShip(_ship);
                _ship.GoToDock();
                GetTree().ChangeSceneToFile("res://ShipBuilder/ShipBuilder.tscn");
            }
        }
    }
}
