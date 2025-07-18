using Godot;
using System;
using System.Collections.Generic;

public enum FloorTileType
{
    Wood,
    Iron
}

public partial class ShipBuilder : Node2D
{
    [Export] public Sprite2D GhostTile;
    private Texture2D ghostTileRedTexture;
    private Texture2D ghostTileGreenTexture;

    [Export] public PackedScene FloorTileScene;
    private Ship _ship;
    private FloorTileType currentTile = FloorTileType.Wood;

    private Button buildMenuButton;
    private PanelContainer buildMenuPanel;
    private Button woodTileButton;
    private Button ironTileButton;


    private readonly Dictionary<FloorTileType, PackedScene> tileScenes = new()
    {
        { FloorTileType.Wood, GD.Load<PackedScene>("res://Tiles/WoodFloorTile.tscn") },
        { FloorTileType.Iron, GD.Load<PackedScene>("res://Tiles/IronFloorTile.tscn") }
    };


    private readonly Dictionary<FloorTileType, Dictionary<string, int>> tileCosts = new()
    {
        { FloorTileType.Wood, new Dictionary<string, int> { { "Wood", 4 } } },
        { FloorTileType.Iron, new Dictionary<string, int> { { "Iron", 6 }, { "Wood", 2 } } }
    };

    const int TILE_SIZE = 32;

    public override void _Ready()
    {
        ghostTileRedTexture = GD.Load<Texture2D>("res://Assets/ghost_tile.png");
        ghostTileGreenTexture = GD.Load<Texture2D>("res://Assets/ghost_tile_green.png");

        if (ShipManager.Instance.CurrentShip == null)
        {
            var shipScene = GD.Load<PackedScene>("res://Ship.tscn");
            _ship = shipScene.Instantiate<Ship>();
            AddChild(_ship);
        }
        else
        {
            _ship = ShipManager.Instance.CurrentShip;

            if (_ship.GetParent() != this)
            {
                _ship.Reparent(this);
            }
        }

        buildMenuButton = GetNode<Button>("UI/BuildMenuButton");
        buildMenuPanel = GetNode<PanelContainer>("UI/BuildMenu");
        woodTileButton = GetNode<Button>("UI/BuildMenu/TabContainer/Floors/Wood");
        ironTileButton = GetNode<Button>("UI/BuildMenu/TabContainer/Floors/Iron");
        buildMenuButton.Pressed += OnBuildMenuButtonPressed;
        woodTileButton.Pressed += () => SelectTile(FloorTileType.Wood);
        ironTileButton.Pressed += () => SelectTile(FloorTileType.Iron);

        buildMenuPanel.Visible = false;
    }

    public override void _Process(double delta)
    {
        Vector2 mouseWorld = GetGlobalMousePosition();
        Vector2I gridPos = WorldToGrid(mouseWorld);
        Vector2I snappedPos = GridToWorld(gridPos);

        bool canPlace = CanPlaceTile(snappedPos);

        GhostTile.Position = snappedPos;
        GhostTile.Texture = canPlace ? ghostTileGreenTexture : ghostTileRedTexture;

    }

    public override void _Input(InputEvent @event)
    {

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                GD.Print($"mouse: {mouseEvent.Position}");
                Vector2I gridPos = WorldToGrid(mouseEvent.Position);
                TryPlaceTile(gridPos);
            }
        }
        else if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.B && keyEvent.Pressed && !keyEvent.Echo)
            {
                ShipManager.Instance.SetShip(_ship);
                _ship.Position = _ship.GetCenterWorldPosition();
                GD.Print($"center ship {_ship.Position}");
                _ship.EnableMovement();
                GetTree().ChangeSceneToFile("res://Game.tscn");
            }

            else if (keyEvent.Keycode == Key.W && keyEvent.Pressed && !keyEvent.Echo)
            {
                currentTile = FloorTileType.Wood;
            }

            else if (keyEvent.Keycode == Key.I && keyEvent.Pressed && !keyEvent.Echo)
            {
                currentTile = FloorTileType.Iron;
            }
        }
    }

    public override void _Draw()
    {
        var viewSize = GetViewportRect().Size;
        Color gridColor = new Color(1, 1, 1, 0.05f);

        for (int x = 0; x < viewSize.X; x += TILE_SIZE)
        {
            DrawLine(new Vector2(x, 0), new Vector2(x, viewSize.Y), gridColor, 1);
        }

        for (int y = 0; y < viewSize.Y; y += TILE_SIZE)
        {
            DrawLine(new Vector2(0, y), new Vector2(viewSize.X, y), gridColor, 1);
        }
    }

    private void TryPlaceTile(Vector2I gridPos)
    {
        if (_ship.Slots.ContainsKey(gridPos))
            return;
        if (!tileScenes.ContainsKey(currentTile))
        {
            GD.PrintErr($"Tile type '{currentTile}' not found!");
            return;
        }
        var cost = tileCosts[currentTile];
        if (!HasEnoughResources(cost))
        {
            GD.Print("Not enough resources to build this tile.");
            return;
        }


        var tilePos = GridToWorld(gridPos);

        if (!CanPlaceTile(tilePos))
        {
            GD.Print("Tile cannot be placed here");
            return;
        }

        DeductResources(cost);

        FloorTile tile = tileScenes[currentTile].Instantiate<FloorTile>();
        _ship.SetFloor(tilePos, tile);
        _ship.UpdateBounds(tilePos);

    }

    private bool CanPlaceTile(Vector2I position)
    {
        if (_ship.Slots.Count == 0)
            return true;

        Vector2I[] neighbors = new Vector2I[]
        {
            new Vector2I(0, -TILE_SIZE),
            new Vector2I(0, TILE_SIZE),
            new Vector2I(-TILE_SIZE, 0),
            new Vector2I(TILE_SIZE, 0)
        };

        foreach (var offset in neighbors)
        {
            var neighborPos = position + offset;
            if (_ship.Slots.ContainsKey(neighborPos))
                return true;
        }

        return false;
    }


    private bool HasEnoughResources(Dictionary<string, int> cost)
    {
        foreach (var resource in cost)
        {
            var resourceName = resource.Key;
            var amountRequired = resource.Value;
            int playerAmount = GetPlayerResourceAmount(resourceName);

            if (playerAmount < amountRequired)
                return false;
        }
        return true;
    }

    private void DeductResources(Dictionary<string, int> cost)
    {
        foreach (var resource in cost)
        {
            var resourceName = resource.Key;
            var amount = resource.Value;
            DecreasePlayerResource(resourceName, amount);
        }
    }

    private int GetPlayerResourceAmount(string resourceName)
    {
        return resourceName switch
        {
            "Wood" => _ship.playerResourceManager.Wood,
            "Coal" => _ship.playerResourceManager.Coal,
            "Iron" => _ship.playerResourceManager.Iron,
            "Copper" => _ship.playerResourceManager.Copper,
            _ => 0
        };
    }

    private void DecreasePlayerResource(string resourceName, int amount)
    {
        switch (resourceName)
        {
            case "Wood":
                _ship.playerResourceManager.DecreaseWood(amount);
                break;
            case "Coal":
                _ship.playerResourceManager.DecreaseCoal(amount);
                break;
            case "Iron":
                _ship.playerResourceManager.DecreaseIron(amount);
                break;
            case "Copper":
                _ship.playerResourceManager.DecreaseCopper(amount);
                break;
        }
    }

    private Vector2I WorldToGrid(Vector2 worldPos)
    {
        return new Vector2I(
            Mathf.FloorToInt(worldPos.X / TILE_SIZE),
            Mathf.FloorToInt(worldPos.Y / TILE_SIZE)
        );
    }

    private Vector2I GridToWorld(Vector2I gridPos)
    {
        return new Vector2I(
            gridPos.X * TILE_SIZE,
            gridPos.Y * TILE_SIZE
        );
    }
    // --- UI Callbacks ---

    private void OnBuildMenuButtonPressed()
    {
        buildMenuButton.Visible = false;
        buildMenuPanel.Visible = true;
    }

    private void SelectTile(FloorTileType tileType)
    {
        if (tileScenes.ContainsKey(tileType))
        {
            currentTile = tileType;
            buildMenuPanel.Visible = false;
            buildMenuButton.Visible = true;
        }
        else
        {
            GD.PrintErr($"Unknown tile type selected: {tileType}");
        }
    }
}
