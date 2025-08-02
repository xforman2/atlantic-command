using Godot;
using System;

public partial class ShipManager : Node
{
    public static ShipManager Instance { get; private set; }

    public PlayerShip CurrentShip { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        CurrentShip = null;
    }

    public void SetShip(PlayerShip ship)
    {

        CurrentShip = ship;
        if (CurrentShip != null && CurrentShip.GetParent() != this)
        {
            CurrentShip.Reparent(this);
        }
    }
}
