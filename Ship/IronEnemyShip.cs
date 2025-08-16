using Godot;
using System;

public partial class IronEnemyShip : EnemyShip
{
    private const int DROP_AMOUNT = 20;

    protected override void InitializeShipSpecifics()
    {
        AddFloor(new Vector2I(-16, 16), FloorTileType.Iron);
        AddFloor(new Vector2I(-16, 48), FloorTileType.Iron);
        AddFloor(new Vector2I(-16, 80), FloorTileType.Iron);
        AddFloor(new Vector2I(-16, 112), FloorTileType.Iron);
        AddFloor(new Vector2I(16, 16), FloorTileType.Iron);
        AddFloor(new Vector2I(16, 48), FloorTileType.Iron);
        AddFloor(new Vector2I(16, 80), FloorTileType.Iron);
        AddFloor(new Vector2I(16, 112), FloorTileType.Iron);

        PlaceStructure(new Vector2I(0, 32), GunType.Cannon, 0);

    }

    public override void DropResources()
    {
        ShipManager.Instance.CurrentShip.playerResourceManager.IncreaseResource(ResourceEnum.Tridentis, DROP_AMOUNT);
    }
}
