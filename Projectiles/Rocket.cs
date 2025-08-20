using Godot;
using System;

public partial class Rocket : Projectile
{

    protected override int DefaultDamage => 50;
    [Export] public float Speed { get; set; } = 600;

    public Vector2 Direction { get; set; }

    public override void _Ready()
    {
        BodyShapeEntered += OnBodyShapeEnteredRocket;
        GetTree().CreateTimer(10).Timeout += QueueFree;
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += Direction * Speed * (float)delta;
    }


    protected void OnBodyShapeEnteredRocket(Rid bodyRid, Node2D body,
            long bodyShapeIndex, long localShapeIndex)
    {
        if (body is EnemyShip)
        {
            base.OnBodyShapeEntered(bodyRid, body, bodyShapeIndex, localShapeIndex);

        }

    }
}
