using Godot;
using System;

public partial class ShipManager : Node
{
    public static ShipManager Instance { get; private set; }

    public Ship CurrentShip { get; private set; }
    public Vector2 LastWorldPosition = Vector2.Zero;

    public override void _Ready()
    {
        Instance = this;
        CurrentShip = null;
    }


    public void SetShip(Ship ship)
    {

        CurrentShip = ship;
        if (CurrentShip != null && CurrentShip.GetParent() != this)
        {
            CurrentShip.Reparent(this);
        }
    }
    public Vector2 GetSavedShipWorldPosition()
    {
        return LastWorldPosition;
    }
    public void SetShipWorldPosition(Vector2 position)
    {
        LastWorldPosition = position;
    }
}
