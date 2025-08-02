using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Game : Node2D
{
    private Ship _ship;
    private TileMapLayer _groundLayer;
    private TileMapLayer _environmentLayer;

    [Export] public int ChunkSize = 32;
    [Export] public int ActiveChunkRadius = 2;
    [Export] public float NoiseScale = 0.05f;
    public const float SandThreshold = 0.15f;
    public const float GrassThreshold = 0.2f;
    public const float TreeThreshold = 0.5f;
    public const float RockThreshold = -0.5f;
    [Export] public NoiseTexture2D HeightNoise;
    [Export] public NoiseTexture2D EnvironmentNoise;

    private Dictionary<Vector2I, bool> _loadedChunks = new();
    private Vector2I _lastChunkPos = new(-9999, -9999);

    public override void _Ready()
    {
        _groundLayer = GetNode<TileMapLayer>("GroundLayer");
        _environmentLayer = GetNode<TileMapLayer>("EnvironmentLayer");
        var buildButton = GetNode<Button>("GameOverlay/BuildButton");
        buildButton.Pressed += EnterBuildMode;

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

    }

    public override void _Process(double delta)
    {
        if (_ship == null || _groundLayer == null || HeightNoise == null)
            return;

        Vector2I tileSize = _groundLayer.TileSet.TileSize;
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
        var heightNoise = HeightNoise.Noise;
        var environmentNoise = EnvironmentNoise.Noise;
        Vector2I start = chunkPos * ChunkSize;

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;
        float minEnv = float.MaxValue;
        float maxEnv = float.MinValue;

        for (int x = 0; x < ChunkSize; x++)
        {
            for (int y = 0; y < ChunkSize; y++)
            {
                Vector2I tilePos = start + new Vector2I(x, y);
                float heightValue = heightNoise.GetNoise2D(tilePos.X * NoiseScale, tilePos.Y * NoiseScale);
                float envValue = environmentNoise.GetNoise2D(tilePos.X * NoiseScale, tilePos.Y * NoiseScale);

                minHeight = MathF.Min(minHeight, heightValue);
                maxHeight = MathF.Max(maxHeight, heightValue);
                minEnv = MathF.Min(minEnv, envValue);
                maxEnv = MathF.Max(maxEnv, envValue);

                GroundTextureEnum groundId = GetGroundType(heightValue);
                _groundLayer.SetCell(tilePos, (int)groundId, Vector2I.Zero);

                if (groundId == GroundTextureEnum.Sand)
                {
                    if (envValue > TreeThreshold)
                    {
                        _environmentLayer.SetCell(tilePos, (int)EnvironmentTextureEnum.Tree, Vector2I.Zero);
                    }
                    else if (envValue < RockThreshold)
                    {
                        _environmentLayer.SetCell(tilePos, (int)EnvironmentTextureEnum.Rock, Vector2I.Zero);
                    }
                }
            }
        }

    }

    private void UnloadChunk(Vector2I chunkPos)
    {
        Vector2I start = chunkPos * ChunkSize;

        for (int x = 0; x < ChunkSize; x++)
        {
            for (int y = 0; y < ChunkSize; y++)
            {
                Vector2I tilePos = start + new Vector2I(x, y);
                _groundLayer.SetCell(tilePos, -1, Vector2I.Zero);
                _environmentLayer.SetCell(tilePos, -1, Vector2I.Zero);
            }
        }
    }
    private void EnterBuildMode()
    {
        ShipManager.Instance.SetShip(_ship);
        _ship.GoToDock();
        GetTree().ChangeSceneToFile("res://ShipBuilder/ShipBuilder.tscn");

    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.B && keyEvent.Pressed && !keyEvent.Echo)
            {
                EnterBuildMode();
            }
        }
    }

    private GroundTextureEnum GetGroundType(float heightValue) => heightValue switch
    {
        >= GrassThreshold => GroundTextureEnum.Grass,
        >= SandThreshold => GroundTextureEnum.Sand,
        _ => GroundTextureEnum.Water
    };
}
