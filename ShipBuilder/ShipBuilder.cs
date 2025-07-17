using Godot;
using System;

public partial class ShipBuilder : Node2D
{
    [Export] public Sprite2D GhostTile;
    [Export] public PackedScene FloorTileScene;
    private Ship ship;

    const int TILE_SIZE = 128;

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
        }
    }

    private void TryPlaceTile(Vector2I gridPos)
    {
        if (ship.Tiles.ContainsKey(gridPos))
            return;
        FloorTile tile = FloorTileScene.Instantiate<FloorTile>();

        tile.Position = GridToWorld(gridPos);
        ship.AddTile(tile, gridPos);

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
        return new Vector2(
            gridPos.X * TILE_SIZE,
            gridPos.Y * TILE_SIZE
        );
    }
}
