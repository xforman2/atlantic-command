using Godot;
using System;

public partial class CannonBall : RigidBody2D
{
    public override void _Ready()
    {
        GetTree().CreateTimer(5).Timeout += QueueFree;
    }

    private void OnBodyEntered(Node body)
    {
        GD.Print("Hit: ", body.Name);
        QueueFree();
    }
}
