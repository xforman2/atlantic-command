using Godot;
using System;

public partial class FloorTile : Node2D
{
    [Export]
    public int HP { get; set; } = 10;

    public Vector2I GridPosition { get; set; }

    public void Init(Vector2I gridPos)
    {
        GridPosition = gridPos;
        Position = new Vector2(gridPos.X * 128, gridPos.Y * 128);
    }

    public void TakeDamage(int amount)
    {
        HP -= amount;
        if (HP <= 0)
        {
            QueueFree();
        }
    }
}
