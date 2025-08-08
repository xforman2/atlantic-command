using Godot;
using System;

public partial class Rocket : Projectile
{

    protected override int DefaultDamage => 25;
    [Export] public float Speed { get; set; } = 500f;

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


    protected new void OnBodyShapeEntered(Rid bodyRid, Node2D body,
            long bodyShapeIndex, long localShapeIndex)
    {
        if (body is EnemyShip)
        {
            GD.Print("here");
            base.OnBodyShapeEntered(bodyRid, body, bodyShapeIndex, localShapeIndex);

        }

    }
}
