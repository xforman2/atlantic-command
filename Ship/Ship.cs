using Godot;
using System.Collections.Generic;

public partial class Ship : Node2D
{
    public Dictionary<Vector2I, ShipSlot> Slots = new();


    [Export]
    public int Speed { get; set; } = 300;

    public PlayerResourceManager playerResourceManager;

    private PackedScene _shipSlotScene;
    private Camera2D _camera;

    private int _minX = int.MaxValue;
    private int _maxX = int.MinValue;
    private int _minY = int.MaxValue;
    private int _maxY = int.MinValue;

    public void Init()
    {

        GD.Print("SHIP INIT");
        if (playerResourceManager == null)
        {
            playerResourceManager = new PlayerResourceManager();
        }
        _shipSlotScene = GD.Load<PackedScene>("res://Tiles/ShipSlot.tscn");
        playerResourceManager.IncreaseCoal(100);
        playerResourceManager.IncreaseWood(100);
        playerResourceManager.IncreaseCopper(100);
        playerResourceManager.IncreaseIron(100);

    }

    public void SetFloor(Vector2I gridPos, FloorTile floor)
    {
        if (!Slots.ContainsKey(gridPos))
            AddSlot(gridPos);

        Slots[gridPos].SetFloor(floor);
    }

    public Vector2 GetCenterWorldPosition()
    {
        if (_minX > _maxX || _minY > _maxY)
        {
            GD.PrintErr("No tiles placed yet. Bounds are invalid.");
            return Vector2.Zero;
        }

        float centerX = (_minX + _maxX) / 2f;
        float centerY = (_minY + _maxY) / 2f;
        return new Vector2(centerX, centerY);
    }
    public void UpdateBounds(Vector2I worldPos)
    {
        if (worldPos.X < _minX) _minX = worldPos.X;
        if (worldPos.X > _maxX) _maxX = worldPos.X;
        if (worldPos.Y < _minY) _minY = worldPos.Y;
        if (worldPos.Y > _maxY) _maxY = worldPos.Y;
    }

    public void EnableMovement()
    {
        _camera.Enabled = true;
    }

    public void DisableMovement()
    {
        _camera.Enabled = false;
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

    public override void _Process(double delta)
    {
        if (!_camera.Enabled) return;

        Vector2 direction = Vector2.Zero;

        if (Input.IsActionPressed("ui_up"))
            direction.Y -= 1;
        if (Input.IsActionPressed("ui_down"))
            direction.Y += 1;
        if (Input.IsActionPressed("ui_left"))
            direction.X -= 1;
        if (Input.IsActionPressed("ui_right"))
            direction.X += 1;

        if (direction != Vector2.Zero)
        {
            direction = direction.Normalized();
            Position += direction * Speed * (float)delta;
        }
    }
}
