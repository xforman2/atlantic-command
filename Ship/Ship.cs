using Godot;
using System.Collections.Generic;

public partial class Ship : Node2D
{
    public Dictionary<Vector2I, FloorTile> Tiles = new();

    [Export] public int MaxHP { get; set; } = 100;
    public int CurrentHP { get; set; }

    public override void _Ready()
    {
        CurrentHP = MaxHP;
    }

    public void AddTile(FloorTile tile, Vector2I gridPos)
    {
        tile.Init(gridPos);
        AddChild(tile);
        Tiles[gridPos] = tile;
    }

    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        if (CurrentHP <= 0)
            Sink();
    }

    private void Sink()
    {
        GD.Print("Ship sunk!");
    }

    private Vector2 GridToWorld(Vector2I gridPos)
    {
        const int TILE_SIZE = 128;
        return new Vector2(gridPos.X * TILE_SIZE, gridPos.Y * TILE_SIZE);
    }
}
