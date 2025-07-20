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

        if (playerResourceManager == null)
        {
            playerResourceManager = new PlayerResourceManager();
        }
        _shipSlotScene = GD.Load<PackedScene>("res://Tiles/ShipSlot.tscn");
        _camera = GetNode<Camera2D>("Camera");
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

    private Vector2 GetCenterWorldPosition()
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
    public void UpdateBounds(Vector2I worldPos, int tileSize)
    {
        if (worldPos.X < _minX) _minX = worldPos.X;
        if ((worldPos.X + tileSize) > _maxX) _maxX = worldPos.X + tileSize;
        if (worldPos.Y < _minY) _minY = worldPos.Y;
        if ((worldPos.Y + tileSize) > _maxY) _maxY = worldPos.Y + tileSize;
    }

    public void GoOutOfDock()
    {
        GlobalPosition = GetCenterWorldPosition();
        _camera.Enabled = true;
        // since go out of dock means that the position of the ship will be the middle position of all the tiles
        // we need to make sure that the tiles do not move with the ship (they are children of the ship), so we 
        // decrease with the current position of the ship
        foreach (var slot in Slots.Values)
        {
            slot.Position -= this.Position;

        }
    }

    public void GoToDock()
    {
        GlobalPosition = Vector2.Zero;
        _camera.Enabled = false;
        // go to dock will make the position of the ship (0, 0), and we need to put positions of all tiles to previous
        // positions, and these positions are inside of the Slot.Keys.
        foreach (var (position, slot) in Slots)
        {
            slot.Position = position;
        }
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
        if (!_camera?.Enabled ?? false) return;

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
