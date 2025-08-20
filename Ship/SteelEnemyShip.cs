using Godot;
using System;

public partial class SteelEnemyShip : EnemyShip
{
    private const int DROP_AMOUNT = 80;

    protected override void InitializeShipSpecifics()
    {
        AddFloor(new Vector2I(-16, 16), FloorTileType.Steel);
        AddFloor(new Vector2I(-16, 48), FloorTileType.Steel);
        AddFloor(new Vector2I(-16, 80), FloorTileType.Steel);
        AddFloor(new Vector2I(-16, 112), FloorTileType.Steel);
        AddFloor(new Vector2I(16, 16), FloorTileType.Steel);
        AddFloor(new Vector2I(16, 48), FloorTileType.Steel);
        AddFloor(new Vector2I(16, 80), FloorTileType.Steel);
        AddFloor(new Vector2I(16, 112), FloorTileType.Steel);

        PlaceStructure(new Vector2I(0, 32), GunType.Torpedo, 0);
    }

    public override void DropResources()
    {
        ShipManager.Instance.CurrentShip.playerResourceManager.IncreaseResource(ResourceEnum.Tridentis, DROP_AMOUNT);
    }
}
