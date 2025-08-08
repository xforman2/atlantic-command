using Godot;
using System;

public partial class Torpedo : Projectile
{

    [Export] public float Speed { get; set; } = 400;
    protected override int DefaultDamage => 75;

    public Vector2 Direction { get; set; }

    public override void _Ready()
    {
        BodyShapeEntered += OnBodyShapeEntered;
        GetTree().CreateTimer(10).Timeout += QueueFree;
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += Direction * Speed * (float)delta;
    }
}
