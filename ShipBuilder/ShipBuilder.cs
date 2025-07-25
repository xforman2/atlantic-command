using Godot;
using System;
using System.Collections.Generic;

public enum FloorTileType
{
    Wood,
    Iron,
    Steel
}

public enum BuildMode
{
    None,
    Floor,
    Gun
}

public enum GunType
{
    Cannon
}

public partial class ShipBuilder : Node2D
{
    [Export] public Sprite2D GhostTile;
    private Texture2D ghostTileRedTexture;
    private Texture2D ghostTileGreenTexture;

    private Ship _ship;
    private FloorTileType currentTile = FloorTileType.Wood;
    private GunType currentGun = GunType.Cannon;
    private BuildMode currentBuildMode = BuildMode.None;

    private Button buildMenuButton;
    private PanelContainer buildMenuPanel;
    private Button woodTileButton;
    private Button ironTileButton;
    private Button steelTileButton;
    private Button cannonGunButton;


    private readonly Dictionary<FloorTileType, PackedScene> tileScenes = new()
    {
        { FloorTileType.Wood, GD.Load<PackedScene>("res://Tiles/WoodFloorTile.tscn") },
        { FloorTileType.Iron, GD.Load<PackedScene>("res://Tiles/IronFloorTile.tscn") },
        { FloorTileType.Steel, GD.Load<PackedScene>("res://Tiles/SteelFloorTile.tscn") }
    };

    private readonly Dictionary<GunType, PackedScene> gunScenes = new()
    {
        { GunType.Cannon, GD.Load<PackedScene>("res://Guns/Cannon2x2.tscn") }
    };

    private readonly Dictionary<FloorTileType, Texture2D> tilePreviewTextures = new(){

        { FloorTileType.Wood, GD.Load<Texture2D>("res://Assets/floor_wood.png") },
        { FloorTileType.Iron, GD.Load<Texture2D>("res://Assets/floor_iron.png") },
        { FloorTileType.Steel, GD.Load<Texture2D>("res://Assets/floor_steel.png") },
    };


    private readonly Dictionary<GunType, Texture2D> gunPreviewTextures = new(){

        { GunType.Cannon, GD.Load<Texture2D>("res://Assets/cannon_64x64.png") }
    };


    private readonly Dictionary<FloorTileType, Dictionary<string, int>> tileCosts = new()
    {
        { FloorTileType.Wood, new Dictionary<string, int> { { "Wood", 4 } } },
        { FloorTileType.Iron, new Dictionary<string, int> { { "Iron", 6 }, { "Wood", 2 } } },
        { FloorTileType.Steel, new Dictionary<string, int> { { "Iron", 6 }, { "Wood", 2 } } }
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
        steelTileButton = GetNode<Button>("UI/BuildMenu/TabContainer/Floors/Steel");
        cannonGunButton = GetNode<Button>("UI/BuildMenu/TabContainer/Guns/Cannon");
        buildMenuButton.Pressed += OnBuildMenuButtonPressed;
        woodTileButton.Pressed += () => SelectFloorTile(FloorTileType.Wood);
        ironTileButton.Pressed += () => SelectFloorTile(FloorTileType.Iron);
        steelTileButton.Pressed += () => SelectFloorTile(FloorTileType.Steel);
        cannonGunButton.Pressed += () => SelectGun(GunType.Cannon);

        buildMenuPanel.Visible = false;
    }

    public override void _Process(double delta)
    {
        Vector2 mouseWorld = GetGlobalMousePosition();
        Vector2I gridPos = WorldToGrid(mouseWorld);
        Vector2I snappedPos = GridToWorld(gridPos);

        bool canPlace = CanPlaceTile(snappedPos);

        GhostTile.Position = snappedPos;
        GhostTile.Modulate = canPlace ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);

        switch (currentBuildMode)
        {
            case BuildMode.Floor:
                GhostTile.Texture = tilePreviewTextures[currentTile];
                break;

            case BuildMode.Gun:
                GhostTile.Texture = gunPreviewTextures[currentGun];
                break;

            default:
                GhostTile.Texture = null;
                break;
        }
    }

    public override void _Input(InputEvent @event)
    {

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                Vector2I gridPos = WorldToGrid(mouseEvent.Position);

                switch (currentBuildMode)
                {
                    case BuildMode.Floor:
                        TryPlaceTile(gridPos);
                        break;
                    case BuildMode.Gun:
                        TryPlaceGun(gridPos);
                        break;
                }
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
            {
                Vector2I gridPos = WorldToGrid(mouseEvent.Position);
                TryRemoveTile(gridPos);
            }
        }
        else if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.B && keyEvent.Pressed && !keyEvent.Echo)
            {
                ShipManager.Instance.SetShip(_ship);
                _ship.GoOutOfDock();
                GetTree().ChangeSceneToFile("res://Game.tscn");
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

        var tilePos = GridToWorld(gridPos);

        if (!tileScenes.ContainsKey(currentTile))
        {
            GD.PrintErr($"Tile type '{currentTile}' not found!");
            return;
        }
        var cost = tileCosts[currentTile];
        if (!HasEnoughResources(cost))
        {
            GD.PrintErr("Not enough resources to build this tile.");
            return;
        }

        if (!_ship.CanAddFloor(tilePos))
        {
            GD.PrintErr("Tile cannot be placed here");
            return;
        }

        DeductResources(cost);

        FloorTile tile = tileScenes[currentTile].Instantiate<FloorTile>();
        tile.Position = tilePos;
        _ship.AddFloor(tilePos, tile);
        _ship.UpdateBounds(tilePos, TILE_SIZE);

    }
    private void TryPlaceGun(Vector2I gridPos)
    {

        var worldPos = GridToWorld(gridPos);
        if (!_ship.CanPlaceStructure(currentGun, worldPos))
        {
            GD.Print("Cannot place gun here.");
            return;
        }

        BuildableStructure structure = gunScenes[currentGun].Instantiate<BuildableStructure>();
        structure.Init(worldPos);


        _ship.PlaceStructure(structure);

        GD.Print("Placed a gun.");
    }

    private void TryRemoveTile(Vector2I gridPos)
    {
        var tilePos = GridToWorld(gridPos);

        if (!_ship.Floors.ContainsKey(tilePos))
        {
            GD.Print("Ship slot does not exist");
            return;
        }


        FloorTile floor = _ship.Floors[tilePos];

        if (floor != null)
        {
            floor.QueueFree();
        }

        _ship.Floors.Remove(tilePos);

    }

    private bool CanPlaceTile(Vector2I position)
    {
        switch (currentBuildMode)
        {
            case BuildMode.Floor:
                return _ship.CanAddFloor(position);

            case BuildMode.Gun:
                return _ship.CanPlaceStructure(currentGun, position);

            default:
                return false;
        }
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

    private void SelectFloorTile(FloorTileType tileType)
    {
        if (tileScenes.ContainsKey(tileType))
        {
            currentBuildMode = BuildMode.Floor;
            currentTile = tileType;
            buildMenuPanel.Visible = false;
            buildMenuButton.Visible = true;
        }
        else
        {
            GD.PrintErr($"Unknown floor tile type selected: {tileType}");
        }
    }



    private void SelectGun(GunType gunType)
    {
        if (gunScenes.ContainsKey(gunType))
        {
            currentBuildMode = BuildMode.Gun;
            currentGun = gunType;
            buildMenuPanel.Visible = false;
            buildMenuButton.Visible = true;
        }
        else
        {
            GD.PrintErr($"Unknown gun type selected: {gunType}");
        }
    }
}
