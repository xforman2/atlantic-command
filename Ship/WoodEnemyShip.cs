using Godot;

public partial class WoodEnemyShip : EnemyShip
{
    private const int DROP_AMOUNT = 10;

    protected override void InitializeShipSpecifics()
    {
        AddFloor(new Vector2I(-16, 16), FloorTileType.Wood);
        AddFloor(new Vector2I(-16, 48), FloorTileType.Wood);
        AddFloor(new Vector2I(-16, 80), FloorTileType.Wood);
        AddFloor(new Vector2I(-16, 112), FloorTileType.Wood);
        AddFloor(new Vector2I(16, 16), FloorTileType.Wood);
        AddFloor(new Vector2I(16, 48), FloorTileType.Wood);
        AddFloor(new Vector2I(16, 80), FloorTileType.Wood);
        AddFloor(new Vector2I(16, 112), FloorTileType.Wood);
        PlaceStructure(new Vector2I(0, 32), GunType.Cannon, 0);
    }

    public override void DropResources()
    {
        ShipManager.Instance.CurrentShip.playerResourceManager.IncreaseResource(ResourceEnum.Tridentis, DROP_AMOUNT);
    }
}
