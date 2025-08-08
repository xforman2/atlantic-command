using Godot;
using System;

public abstract partial class FloorTile : Node2D
{
    protected virtual int DefaultHP => 100;

    public int HP { get; private set; }
    public Vector2I Origin { get; set; }
    public BuildableStructure StructureOnTop { get; set; }

    public void Init(Vector2I position)
    {
        Position = position;
        Origin = position;
        HP = DefaultHP;
    }
    public void TakeDamage(int damage)
    {
        HP -= damage;

        if (HP <= 0)
        {
            OnDestroyed();
        }
    }

    protected virtual void OnDestroyed()
    {
        if (StructureOnTop != null)
        {
            GD.Print($"Destroying structure on top: {StructureOnTop.Name}");
            StructureOnTop.QueueFree();
        }

        QueueFree();
    }
}
