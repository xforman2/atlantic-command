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
    [Export] public PackedScene FloorTileScene;
    private Ship ship;
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

    const int TILE_SIZE = 32;

    public override void _Ready()
    {
        if (ShipManager.Instance.CurrentShip == null)
        {
            var shipScene = GD.Load<PackedScene>("res://Ship.tscn");
            ship = shipScene.Instantiate<Ship>();
            ShipManager.Instance.SetShip(ship);
            AddChild(ship);
        }
        else
        {
            ship = ShipManager.Instance.CurrentShip;

            if (ship.GetParent() != this)
            {
                ship.Reparent(this);
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
        Vector2 snappedPos = GridToWorld(gridPos);

        GhostTile.Position = snappedPos;

    }

    public override void _Input(InputEvent @event)
    {

        if (@event is InputEventMouseButton mouseEvent)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                Vector2I gridPos = WorldToGrid(mouseEvent.Position);
                TryPlaceTile(gridPos);
            }
        }
        else if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.B && keyEvent.Pressed && !keyEvent.Echo)
            {
                ShipManager.Instance.SetShip(ship);
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
        if (ship.Slots.ContainsKey(gridPos))
            return;
        if (!tileScenes.ContainsKey(currentTile))
        {
            GD.PrintErr($"Tile type '{currentTile}' not found!");
            return;
        }
        FloorTile tile = tileScenes[currentTile].Instantiate<FloorTile>();
        var tilePos = GridToWorld(gridPos);
        ship.SetFloor(tilePos, tile);

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
