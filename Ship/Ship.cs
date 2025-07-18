using Godot;
using System.Collections.Generic;

public partial class Ship : Node2D
{
    public Dictionary<Vector2I, ShipSlot> Slots = new();

    public int CurrentHP { get; set; }
    public int MaxHP = 100;

    public PlayerResourceManager _resourceManager;

    private PackedScene _shipSlotScene;

    public override void _Ready()
    {
        CurrentHP = MaxHP;
        _resourceManager = new PlayerResourceManager();
        _shipSlotScene = GD.Load<PackedScene>("res://Tiles/ShipSlot.tscn");
    }

    public void SetFloor(Vector2I gridPos, FloorTile floor)
    {
        if (!Slots.ContainsKey(gridPos))
            AddSlot(gridPos);

        Slots[gridPos].SetFloor(floor);
    }

    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;
        if (CurrentHP <= 0)
            Sink();
    }

    private void AddSlot(Vector2I position)
    {
        if (Slots.ContainsKey(position))
            return;

        var slot = _shipSlotScene.Instantiate<ShipSlot>();
        slot.Init(position);
        AddChild(slot);
        Slots[position] = slot;
    }

    private void Sink()
    {
        GD.Print("Ship sunk!");
    }
}
