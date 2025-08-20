using Godot;
using System;
using System.Linq;

public partial class CannonBall : Projectile
{
    [Export] public float Speed { get; set; } = 400f;
    protected override int DefaultDamage => 25;

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
