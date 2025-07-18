using Godot;
using System;

public partial class FloorTile : Node2D
{
    [Export]
    public int HP { get; set; } = 10;

    public void Init(Vector2 position)
    {
        Position = new Vector2(position.X, position.Y);
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
