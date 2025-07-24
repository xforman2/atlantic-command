using Godot;
using System;

public partial class Game : Node2D
{
    private Ship _ship;
    private TileMapLayer _tileMapLayer;

    [Export]
    public PackedScene ShipScene;

    [Export]
    public Vector2I MapSize = new Vector2I(400, 400);

    [Export]
    public float NoiseScale = 0.1f;

    [Export]
    public float SandThreshold = 0.1f;

    [Export]
    public NoiseTexture2D NoiseTexture2D;

    public override void _Ready()
    {
        _tileMapLayer = GetNode<TileMapLayer>("WorldTileMapLayer");

        _ship = ShipManager.Instance.CurrentShip;
        if (_ship == null)
        {
            _ship = ShipScene.Instantiate<Ship>();
            _ship.Init();
            AddChild(_ship);

            ShipManager.Instance.SetShip(_ship);

            GD.Print("New ship instantiated and assigned to ShipManager.");
        }
        else
        {
            if (_ship.GetParent() != this)
            {
                _ship.Reparent(this);
            }

            GD.Print("Existing ship loaded from ShipManager.");
        }

        _ship.Position = new Vector2(0, 0);
        GenerateMap();
    }

    private void GenerateMap()
    {
        _tileMapLayer.Clear();

        for (int x = 0; x < MapSize.X; x++)
        {
            for (int y = 0; y < MapSize.Y; y++)
            {
                var noise = NoiseTexture2D.Noise;
                float noiseVal = noise.GetNoise2D(x * NoiseScale, y * NoiseScale);
                GD.Print($"{noiseVal}");

                int sourceId = noiseVal > SandThreshold ? 1 : 0;

                _tileMapLayer.SetCell(new Vector2I(x, y), sourceId, new Vector2I(9, 2));


            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent)
        {
            if (keyEvent.Keycode == Key.B && keyEvent.Pressed && !keyEvent.Echo)
            {
                ShipManager.Instance.SetShip(_ship);
                _ship.GoToDock();
                GetTree().ChangeSceneToFile("res://ShipBuilder/ShipBuilder.tscn");
            }
        }
    }
}
