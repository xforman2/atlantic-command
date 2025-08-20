using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Game : Node2D
{
    private InputMode _currentMode = InputMode.None;
    private PlayerShip _ship;
    private TileMapLayer _groundLayer;
    private TileMapLayer _environmentLayer;
    private EnemySpawner _enemySpawner;
    private Sprite2D _rocketGhostTile;
    private Label _modeLabel;
    private Label _coordinatesLabel;
    private Button _backToMenuButton;
    private Timer _modeLabelTimer;

    private bool _isMining = false;
    private float _miningTimer = 0f;
    private float _miningTime = 2f; // seconds it takes to mine a tile, adjust as you want
    private Vector2I _miningCell;
    private TextureProgressBar _miningProgressBar;

    [Export] public int ActiveChunkRadius = 2;
    [Export] public float NoiseScale = 0.05f;
    public const float SandThreshold = 0.15f;
    public const float GrassThreshold = 0.2f;
    public const float TreeThreshold = 0.5f;
    public const float RockThreshold = -0.5f;
    public const float ScrapThreshold = 0.4f;
    public const float IronThreshold = -0.4f;
    [Export] public NoiseTexture2D HeightNoise;
    [Export] public NoiseTexture2D EnvironmentNoise;

    private Dictionary<Vector2I, bool> _loadedChunks = new();
    private Vector2I _lastChunkPos = new(-9999, -9999);
    private bool _canMine;
    private Vector2I _hoverCell;
    private ColorRect _hoverHighlight;
    private ColorRect _diedScreen;
    private Dictionary<int, (ResourceEnum Resource, int Amount)> _tileDrops = new()
    {
        { (int)EnvironmentTextureEnum.Tree, (ResourceEnum.Wood, 1) },
        { (int)EnvironmentTextureEnum.Iron, (ResourceEnum.Iron, 1) },
        { (int)EnvironmentTextureEnum.Scrap, (ResourceEnum.Scrap, 1) },
    };

    public override void _Ready()
    {
        GameState.Instance.LastOrigin = SceneOrigin.Game;

        _rocketGhostTile = GetNode<Sprite2D>("GhostTile");
        _rocketGhostTile.Texture = GD.Load<Texture2D>("res://Assets/ghost_tile_rocket_launcher.png");
        _groundLayer = GetNode<TileMapLayer>("GroundLayer");
        _environmentLayer = GetNode<TileMapLayer>("EnvironmentLayer");
        _enemySpawner = GetNode<EnemySpawner>("EnemySpawner");

        _miningProgressBar = GetNode<TextureProgressBar>("GameOverlay/MiningProgressBar");
        _miningProgressBar.Visible = false;

        _coordinatesLabel = GetNode<Label>("GameOverlay/CoordinatesLabel");
        _modeLabel = GetNode<Label>("GameOverlay/ModeLabel");
        _diedScreen = GetNode<ColorRect>("GameOverlay/YouDied");
        _backToMenuButton = GetNode<Button>("GameOverlay/YouDied/BackToMenuButton");

        var buildButton = GetNode<Button>("GameOverlay/BuildButton");
        buildButton.Pressed += EnterBuildMode;

        _ship = ShipManager.Instance.CurrentShip;
        if (_ship == null)
        {
            var scene = GD.Load<PackedScene>("Ship/PlayerShip.tscn");
            _ship = scene.Instantiate<PlayerShip>();
            AddChild(_ship);
            ShipManager.Instance.ReparentShip(_ship);
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
        _ship.ShipDestroyed += OnShipDestroyedHandler;
        _hoverHighlight = GetNode<ColorRect>("HoverHighlight");
        _hoverHighlight.Visible = false;
        _hoverHighlight.Size = new Vector2(Globals.TILE_SIZE, Globals.TILE_SIZE);

        _modeLabelTimer = new Timer();
        _modeLabelTimer.WaitTime = 3.0f;
        _modeLabelTimer.OneShot = true;
        AddChild(_modeLabelTimer);
        _modeLabelTimer.Timeout += OnTimerTimeout;
        _backToMenuButton.Pressed += OnBackToMenu;
        UpdateModeLabel();
    }

    public override void _Process(double delta)
    {
        if (_ship == null || _groundLayer == null || HeightNoise == null)
            return;
        HandleChunking();
        if (_currentMode == InputMode.Mining)
        {
            UpdateMiningHoverUI();
        }
        if (_isMining)
        {
            HandleMining();
        }
        if (_currentMode == InputMode.RocketFiring)
        {
            _rocketGhostTile.Position = GetGlobalMousePosition();
        }

        _coordinatesLabel.Text = $"{_ship.Position.X:0}    {_ship.Position.Y:0}";
    }

    public override void _ExitTree()
    {
        if (_ship != null)
        {
            _ship.ShipDestroyed -= OnShipDestroyedHandler;
        }
    }

    private void HandleMining()
    {
        if (!_isMining)
            return;

        _miningTimer += (float)GetProcessDeltaTime();
        _miningProgressBar.Value = (_miningTimer / _miningTime) * 100;

        if (_miningTimer >= _miningTime)
        {
            FinishMining();
        }
    }
    private void FinishMining()
    {
        MineTile(_miningCell);
        _isMining = false;
        _miningTimer = 0f;
        _miningProgressBar.Visible = false;
    }
    private void StartMining(Vector2I cell)
    {
        _isMining = true;
        _miningTimer = 0f;
        _miningCell = cell;
        _miningProgressBar.Visible = true;
        _miningProgressBar.Value = 0;
    }

    private void MineTile(Vector2I cell)
    {
        int tileId = _environmentLayer.GetCellSourceId(cell);
        if (!_tileDrops.TryGetValue(tileId, out var drop))
            return;

        _ship.playerResourceManager.IncreaseResource(drop.Resource, drop.Amount);
        _environmentLayer.SetCell(cell, -1, Vector2I.Zero);
        _hoverHighlight.Visible = false;
        _canMine = false;
    }

    private void UpdateMiningHoverUI()
    {
        Vector2 mouseWorld = GetGlobalMousePosition();
        _hoverCell = _environmentLayer.LocalToMap(mouseWorld);

        int tileId = _environmentLayer.GetCellSourceId(_hoverCell);
        bool isMineable = _tileDrops.ContainsKey(tileId);
        float distanceToShip = _ship.Position.DistanceTo(mouseWorld);

        _canMine = isMineable && _ship.IsPointWithinMiningRange(mouseWorld);

        if (isMineable)
        {
            _hoverHighlight.Position = _environmentLayer.MapToLocal(_hoverCell) - new Vector2(Globals.TILE_SIZE / 2, Globals.TILE_SIZE / 2);
            _hoverHighlight.Color = _canMine ? Colors.Green : Colors.Red;
            _hoverHighlight.Visible = true;
        }
        else
        {
            _hoverHighlight.Visible = false;
        }
    }

    private void HandleChunking()
    {
        Vector2I tileSize = _groundLayer.TileSet.TileSize;
        Vector2I currentChunk = new Vector2I(
                Mathf.FloorToInt(_ship.Position.X / (Globals.CHUNK_SIZE * tileSize.X)),
                Mathf.FloorToInt(_ship.Position.Y / (Globals.CHUNK_SIZE * tileSize.Y))
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
        Vector2I start = chunkPos * Globals.CHUNK_SIZE;

        for (int x = 0; x < Globals.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Globals.CHUNK_SIZE; y++)
            {
                Vector2I tilePos = start + new Vector2I(x, y);
                float heightValue = heightNoise.GetNoise2D(tilePos.X * NoiseScale, tilePos.Y * NoiseScale);
                float envValue = environmentNoise.GetNoise2D(tilePos.X * NoiseScale, tilePos.Y * NoiseScale);

                GroundTextureEnum groundId = GetGroundType(heightValue);
                _groundLayer.SetCell(tilePos, (int)groundId, Vector2I.Zero);

                if (groundId == GroundTextureEnum.Sand)
                {
                    if (envValue > TreeThreshold)
                        _environmentLayer.SetCell(tilePos, (int)EnvironmentTextureEnum.Tree, Vector2I.Zero);
                    else if (envValue > ScrapThreshold)
                        _environmentLayer.SetCell(tilePos, (int)EnvironmentTextureEnum.Scrap, Vector2I.Zero);
                    else if (envValue < IronThreshold)
                        _environmentLayer.SetCell(tilePos, (int)EnvironmentTextureEnum.Iron, Vector2I.Zero);
                    else if (envValue < RockThreshold)
                        _environmentLayer.SetCell(tilePos, (int)EnvironmentTextureEnum.Rock, Vector2I.Zero);
                }
            }
        }

        _enemySpawner.SpawnEnemiesInChunk(_ship.Position, chunkPos, _groundLayer);
    }

    private void UnloadChunk(Vector2I chunkPos)
    {
        Vector2I start = chunkPos * Globals.CHUNK_SIZE;

        for (int x = 0; x < Globals.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < Globals.CHUNK_SIZE; y++)
            {
                Vector2I tilePos = start + new Vector2I(x, y);
                _groundLayer.SetCell(tilePos, -1, Vector2I.Zero);
                _environmentLayer.SetCell(tilePos, -1, Vector2I.Zero);
            }
        }

        _enemySpawner.DespawnEnemiesInChunk(chunkPos, _groundLayer);
    }

    private void EnterBuildMode()
    {
        ShipManager.Instance.ReparentShip(_ship);
        _ship.GoToDock();
        GetTree().ChangeSceneToFile("res://ShipBuilder/ShipBuilder.tscn");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            switch (_currentMode)
            {
                case InputMode.Mining:
                    if (_canMine) StartMining(_hoverCell);
                    break;
                case InputMode.RocketFiring:
                    _ship.ShootRockets(GetGlobalMousePosition());
                    break;
            }
        }

        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            switch (keyEvent.Keycode)
            {
                case Key.M:
                    ToggleMode(InputMode.Mining);
                    break;
                case Key.R:
                    ToggleMode(InputMode.RocketFiring);
                    break;
                case Key.H:
                    ToggleMode(InputMode.HpStatus);
                    break;
                case Key.F:
                    _ship.ShootCannons();
                    break;
                case Key.T:
                    _ship.ShootTorpedos();
                    break;
            }
        }
    }

    private void ToggleMode(InputMode mode)
    {
        bool wasSame = (_currentMode == mode);
        _currentMode = wasSame ? InputMode.None : mode;

        if (wasSame)
        {
            _hoverHighlight.Visible = false;
            _rocketGhostTile.Visible = false;
            ToggleStructuresVisibility(true);
            ToggleHpLabelsVisibility(false);
        }
        else
        {
            switch (_currentMode)
            {
                case InputMode.Mining:
                    _hoverHighlight.Visible = true;
                    _rocketGhostTile.Visible = false;
                    ToggleStructuresVisibility(true);
                    ToggleHpLabelsVisibility(false);
                    break;
                case InputMode.RocketFiring:
                    _hoverHighlight.Visible = false;
                    _rocketGhostTile.Visible = true;
                    ToggleStructuresVisibility(true);
                    ToggleHpLabelsVisibility(false);
                    break;
                case InputMode.HpStatus:
                    _hoverHighlight.Visible = false;
                    _rocketGhostTile.Visible = false;
                    ToggleStructuresVisibility(false);
                    ToggleHpLabelsVisibility(true);
                    break;
                case InputMode.None:
                    _hoverHighlight.Visible = false;
                    _rocketGhostTile.Visible = false;
                    ToggleStructuresVisibility(true);
                    ToggleHpLabelsVisibility(false);
                    break;
            }
        }
        UpdateModeLabel();
    }

    private void ToggleStructuresVisibility(bool visible)
    {
        foreach (var enemyShip in _enemySpawner.ActiveEnemies)
        {
            enemyShip.ToggleStructuresVisibility(visible);
        }
        _ship.ToggleStructuresVisibility(visible);
    }

    private void ToggleHpLabelsVisibility(bool visible)
    {
        foreach (var enemyShip in _enemySpawner.ActiveEnemies)
        {
            enemyShip.ToggleHpLabelsVisibility(visible);
        }
        _ship.ToggleHpLabelsVisibility(visible);
    }


    private GroundTextureEnum GetGroundType(float heightValue) => heightValue switch
    {
        >= GrassThreshold => GroundTextureEnum.Grass,
        >= SandThreshold => GroundTextureEnum.Sand,
        _ => GroundTextureEnum.Water
    };

    private void UpdateModeLabel()
    {
        string modeText;
        switch (_currentMode)
        {
            case InputMode.Mining:
                modeText = "Mining";
                break;
            case InputMode.RocketFiring:
                modeText = "Rocket Firing";
                break;
            case InputMode.HpStatus:
                modeText = "HP Status";
                break;
            case InputMode.None:
            default:
                modeText = "No Mode";
                break;
        }

        _modeLabel.Visible = true;
        _modeLabelTimer.Start();
        _modeLabel.Text = $"Mode: {modeText}";
    }

    private void OnTimerTimeout()
    {
        _modeLabel.Visible = false;
    }

    private void OnShipDestroyedHandler()
    {
        GD.Print(_diedScreen);
        _diedScreen.Visible = true;
    }

    private void OnBackToMenu()
    {

        GameState.Instance.LastOrigin = SceneOrigin.ShipBuilder;
        if (_ship is not null)
        {
            _ship.QueueFree();
            _ship = null;
            ShipManager.Instance.CurrentShip = null;
        }

        GameState.Instance.HasStartedGame = false;
        if (SaveSystem.HasSave())
        {
            SaveSystem.DeleteSave();
        }

        GetTree().ChangeSceneToFile("res://MainMenu/MainMenu.tscn");

    }
}
