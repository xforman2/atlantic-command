using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

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
    Gun,
    Remove
}

public enum GunType
{
    Cannon,
    RocketLauncher,
    Torpedo,
}

public enum ResourceEnum
{
    Tridentis,
    Wood,
    Iron,
    Scrap
}

public partial class ShipBuilder : Node2D
{
    [Export] public Sprite2D GhostTile;
    private Texture2D ghostTileRemoveTextureValid;
    private Texture2D ghostTileRemoveTextureInvalid;

    private PlayerShip _ship;
    private FloorTileType currentTile = FloorTileType.Wood;
    private GunType currentGun;
    private BuildMode currentBuildMode = BuildMode.None;

    private Button buildMenuButton;
    private PanelContainer buildMenuPanel;
    private Button woodTileButton;
    private Button ironTileButton;
    private Button steelTileButton;
    private Button cannonGunButton;
    private Button rocketGunButton;
    private Button torpedoGunButton;


    private readonly Dictionary<FloorTileType, Texture2D> tilePreviewTextures = new(){

        { FloorTileType.Wood, GD.Load<Texture2D>("res://Assets/floor_wood.png") },
        { FloorTileType.Iron, GD.Load<Texture2D>("res://Assets/floor_iron_v1.2.0.png") },
        { FloorTileType.Steel, GD.Load<Texture2D>("res://Assets/floor_steel.png") },
    };


    private readonly Dictionary<GunType, Texture2D> gunPreviewTextures = new(){

        { GunType.Cannon, GD.Load<Texture2D>("res://Assets/Canon.png") },
        { GunType.RocketLauncher, GD.Load<Texture2D>("res://Assets/rocket_launcher.png") },
        { GunType.Torpedo, GD.Load<Texture2D>("res://Assets/torpedo_launcher.png") }
    };

    const int TILE_SIZE = 32;

    public override void _Ready()
    {
        ghostTileRemoveTextureValid = GD.Load<Texture2D>("res://Assets/ghost_tile_green.png");
        ghostTileRemoveTextureInvalid = GD.Load<Texture2D>("res://Assets/ghost_tile.png");

        var quitButton = GetNode<Button>("UI/QuitButton");
        quitButton.Pressed += EnterNormalMode;

        _ship = ShipManager.Instance.CurrentShip;

        if (_ship == null)
        {
            var scene = GD.Load<PackedScene>("Ship/PlayerShip.tscn");
            _ship = scene.Instantiate<PlayerShip>();
            AddChild(_ship);
            _ship.DisableCamera();
            GD.Print("New ship instantiated and assigned to ShipManager.");
        }
        else
        {
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
        rocketGunButton = GetNode<Button>("UI/BuildMenu/TabContainer/Guns/RocketLauncher");
        torpedoGunButton = GetNode<Button>("UI/BuildMenu/TabContainer/Guns/Torpedo");
        buildMenuButton.Pressed += OnBuildMenuButtonPressed;
        woodTileButton.Pressed += () => SelectFloorTile(FloorTileType.Wood);
        ironTileButton.Pressed += () => SelectFloorTile(FloorTileType.Iron);
        steelTileButton.Pressed += () => SelectFloorTile(FloorTileType.Steel);
        cannonGunButton.Pressed += () => SelectGun(GunType.Cannon);
        rocketGunButton.Pressed += () => SelectGun(GunType.RocketLauncher);
        torpedoGunButton.Pressed += () => SelectGun(GunType.Torpedo);

        buildMenuPanel.Visible = false;
    }

    public override void _Process(double delta)
    {
        Vector2 mouseWorld = GetGlobalMousePosition();
        Vector2I snappedPos = GetStructureSnappedPosition(mouseWorld);

        bool canModify = CanModifyTile(snappedPos);

        GhostTile.Position = snappedPos;

        switch (currentBuildMode)
        {
            case BuildMode.Floor:
                GhostTile.Modulate = canModify ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
                GhostTile.Texture = tilePreviewTextures[currentTile];
                break;

            case BuildMode.Gun:
                GhostTile.Modulate = canModify ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
                GhostTile.Texture = gunPreviewTextures[currentGun];
                break;
            case BuildMode.Remove:

                GhostTile.Modulate = new Color(1, 1, 1, 0.8f);
                if (_ship.Structures.TryGetValue(snappedPos, out var structure))
                {
                    GhostTile.Texture = ghostTileRemoveTextureValid;
                    GhostTile.Position = structure.Origin;
                    GhostTile.Scale = new Vector2(structure.Size.X / (float)Globals.TILE_SIZE, structure.Size.Y / (float)Globals.TILE_SIZE);
                }
                else if (_ship.Floors.ContainsKey(snappedPos))
                {

                    GhostTile.Texture = ghostTileRemoveTextureValid;
                    GhostTile.Scale = Vector2.One;
                }
                else
                {

                    GhostTile.Texture = ghostTileRemoveTextureInvalid;
                    GhostTile.Scale = Vector2.One;
                }
                break;
            default:
                break;
        }
    }

    public override void _Input(InputEvent @event)
    {

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                Vector2I position = GetStructureSnappedPosition(mouseEvent.Position);

                switch (currentBuildMode)
                {
                    case BuildMode.Floor:
                        TryPlaceTile(position);
                        break;
                    case BuildMode.Gun:
                        TryPlaceGun(position);
                        break;
                    case BuildMode.Remove:
                        TryRemoveTile(position);
                        break;
                    default:
                        break;
                }
            }
            else if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
            {
                Vector2I position = GetStructureSnappedPosition(mouseEvent.Position);
                TryRemoveTile(position);
            }
        }
        else if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            switch (keyEvent.Keycode)
            {
                case Key.B:
                    EnterNormalMode();
                    break;

                case Key.R:
                    GhostTile.RotationDegrees = (GhostTile.RotationDegrees + 90) % 360;
                    break;

                case Key.X:
                    currentBuildMode = BuildMode.Remove;
                    break;
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

    private void TryPlaceTile(Vector2I tilePos)
    {

        if (!Globals.tileScenes.ContainsKey(currentTile))
        {
            GD.PrintErr($"Tile type '{currentTile}' not found!");
            return;
        }
        var cost = Globals.tileCosts[currentTile];
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

        _ship.AddFloor(tilePos, currentTile);
        _ship.UpdateBounds(tilePos);

    }

    private void TryPlaceGun(Vector2I worldPos)
    {
        var occupiedPositions = Globals.GetOccupiedPositions(worldPos, Globals.gunSizes[currentGun]);
        if (!_ship.CanPlaceStructure(occupiedPositions))
        {
            GD.Print("Cannot place gun here.");
            return;
        }

        switch (currentGun)
        {
            case GunType.Cannon:
            case GunType.Torpedo:
                if (!HasClearFiringLine(worldPos, currentGun, GhostTile.RotationDegrees))
                {
                    GD.Print("Cannot place gun here - firing line blocked.");
                    return;
                }
                break;
            case GunType.RocketLauncher:
                break;
        }

        _ship.PlaceStructure(worldPos, currentGun, GhostTile.RotationDegrees);
    }

    private void TryRemoveTile(Vector2I position)
    {
        if (!_ship.Floors.ContainsKey(position))
        {
            GD.PrintErr($"Nothing to remove at {position}");
            return;
        }

        if (_ship.Structures.ContainsKey(position))
        {
            var structure = _ship.Structures[position];

            foreach (var tile in structure.OccupiedPositions)
            {
                _ship.Structures.Remove(tile);
            }

            _ship.StructuresOrigin.Remove(structure.Origin);
            structure.QueueFree();
            return;
        }

        FloorTile floor = _ship.Floors[position].Item1;
        CollisionShape2D collision = _ship.Floors[position].Item2;


        if (floor != null)
        {
            floor.QueueFree();
            collision.QueueFree();
        }

        _ship.Floors.Remove(position);

    }

    private bool CanModifyTile(Vector2I position)
    {
        switch (currentBuildMode)
        {
            case BuildMode.Floor:
                return _ship.CanAddFloor(position);

            case BuildMode.Gun:
                var occupiedPositions = Globals.GetOccupiedPositions(position, Globals.gunSizes[currentGun]);
                switch (currentGun)
                {
                    case GunType.Cannon:
                    case GunType.Torpedo:
                        return _ship.CanPlaceStructure(occupiedPositions)
                            && HasClearFiringLine(position, currentGun, GhostTile.RotationDegrees);
                    case GunType.RocketLauncher:
                        return _ship.CanPlaceStructure(occupiedPositions);
                    default:
                        return false;
                }
            case BuildMode.Remove:
                return _ship.Structures.ContainsKey(position) || _ship.Floors.ContainsKey(position);
            default:
                return false;
        }
    }
    private bool HasClearFiringLine(Vector2I gunPosition, GunType gunType, float rotationDegrees)
    {

        Vector2I actualFiringDirection = GetFiringDirection(rotationDegrees);

        var frontPositions = GetGunFrontPositions(gunPosition, gunType, rotationDegrees);

        foreach (var frontPos in frontPositions)
        {
            Vector2I checkPos = frontPos + (actualFiringDirection * Globals.TILE_SIZE);

            if (_ship.Floors.ContainsKey(checkPos))
            {
                GD.Print($"Firing line blocked at position: {checkPos}");
                return false;
            }
        }

        return true;
    }
    private Vector2I GetFiringDirection(float degrees)
    {
        int normalizedDegrees = ((int)degrees % 360);

        return normalizedDegrees switch
        {
            0 => new Vector2I(0, -1),
            90 => new Vector2I(1, 0),
            180 => new Vector2I(0, 1),
            270 => new Vector2I(-1, 0),
            _ => Vector2I.Zero
        };
    }
    private List<Vector2I> GetGunFrontPositions(Vector2I gunCenter, GunType gunType, float rotationDegrees)
    {
        var occupiedPositions = Globals.GetOccupiedPositions(gunCenter, Globals.gunSizes[gunType]);

        return ((int)rotationDegrees % 360) switch
        {
            0 => occupiedPositions.Where(pos => pos.Y == occupiedPositions.Min(p => p.Y)).ToList(),
            90 => occupiedPositions.Where(pos => pos.X == occupiedPositions.Max(p => p.X)).ToList(),
            180 => occupiedPositions.Where(pos => pos.Y == occupiedPositions.Max(p => p.Y)).ToList(),
            270 => occupiedPositions.Where(pos => pos.X == occupiedPositions.Min(p => p.X)).ToList(),
            _ => new List<Vector2I>()
        };
    }



    private bool HasEnoughResources(Dictionary<ResourceEnum, int> cost)
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

    private void DeductResources(Dictionary<ResourceEnum, int> cost)
    {
        foreach (var resource in cost)
        {
            var resourceName = resource.Key;
            var amount = resource.Value;
            DecreasePlayerResource(resourceName, amount);
        }
    }

    private int GetPlayerResourceAmount(ResourceEnum resourceName)
    {
        return resourceName switch
        {
            ResourceEnum.Wood => _ship.playerResourceManager.Wood,
            ResourceEnum.Scrap => _ship.playerResourceManager.Scrap,
            ResourceEnum.Iron => _ship.playerResourceManager.Iron,
            ResourceEnum.Tridentis => _ship.playerResourceManager.Tridentis,
            _ => 0
        };
    }

    private void DecreasePlayerResource(ResourceEnum resource, int amount)
    {
        switch (resource)
        {
            case ResourceEnum.Wood:
                _ship.playerResourceManager.DecreaseResource(resource, amount);
                break;
            case ResourceEnum.Scrap:
                _ship.playerResourceManager.DecreaseResource(resource, amount);
                break;
            case ResourceEnum.Iron:
                _ship.playerResourceManager.DecreaseResource(resource, amount);
                break;
            case ResourceEnum.Tridentis:
                _ship.playerResourceManager.DecreaseResource(resource, amount);
                break;
        }
    }
    private Vector2I GetStructureSnappedPosition(Vector2 worldPos)
    {
        Vector2I structureSize;
        switch (currentBuildMode)
        {
            case BuildMode.Floor:
            case BuildMode.Remove:
                structureSize = new Vector2I(32, 32);
                break;
            case BuildMode.Gun:
                structureSize = Globals.gunSizes[currentGun];
                break;
            default:
                structureSize = Vector2I.Zero;
                break;
        }

        int tilesWide = structureSize.X / TILE_SIZE;
        int tilesHigh = structureSize.Y / TILE_SIZE;

        int snappedX, snappedY;

        if (tilesWide % 2 == 1) // Odd width (1x1, 3x3, 5x5)
        {
            // Center should be at grid cell center (16, 48, 80, etc.)
            snappedX = Mathf.RoundToInt((worldPos.X - TILE_SIZE / 2) / TILE_SIZE) * TILE_SIZE + (TILE_SIZE / 2);
        }
        else // Even width (2x2, 4x4, 6x6)
        {
            // Center should be at grid intersection (32, 64, 96, etc.)
            snappedX = Mathf.RoundToInt(worldPos.X / TILE_SIZE) * TILE_SIZE;
        }

        if (tilesHigh % 2 == 1) // Odd height
        {
            // Center should be at grid cell center (16, 48, 80, etc.)
            snappedY = Mathf.RoundToInt((worldPos.Y - TILE_SIZE / 2) / TILE_SIZE) * TILE_SIZE + (TILE_SIZE / 2);
        }
        else // Even height
        {
            // Center should be at grid intersection (32, 64, 96, etc.)
            snappedY = Mathf.RoundToInt(worldPos.Y / TILE_SIZE) * TILE_SIZE;
        }

        return new Vector2I(snappedX, snappedY);
    }

    private Vector2I WorldToGrid(Vector2 worldPos)
    {
        return new Vector2I(
                Mathf.FloorToInt(worldPos.X / TILE_SIZE),
                Mathf.FloorToInt(worldPos.Y / TILE_SIZE)
                );
    }

    private Vector2 GridToWorld(Vector2I gridPos)
    {
        // Return the center of the grid cell for 1x1 structures
        // For larger structures, this will be adjusted in the placement logic
        return new Vector2(
                gridPos.X * TILE_SIZE + TILE_SIZE / 2,
                gridPos.Y * TILE_SIZE + TILE_SIZE / 2
                );
    }

    private void EnterNormalMode()
    {
        ShipManager.Instance.SetShip(_ship);
        _ship.GoOutOfDock();
        GetTree().ChangeSceneToFile("res://Game.tscn");
    }

    // --- UI Callbacks ---

    private void OnBuildMenuButtonPressed()
    {
        buildMenuButton.Visible = false;
        buildMenuPanel.Visible = true;
    }

    private void SelectFloorTile(FloorTileType tileType)
    {
        if (Globals.tileScenes.ContainsKey(tileType))
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
        if (Globals.gunScenes.ContainsKey(gunType))
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
