using Godot;
using System.Threading;
using System.Threading.Tasks;

public partial class Minimap : SubViewport
{
    private Ship _ship;
    private Camera2D _camera;

    public override void _Ready()
    {
        _ship = ShipManager.Instance.CurrentShip;
        _camera = GetNode<Camera2D>("MinimapCamera");
        World2D = GetTree().Root.World2D;

        if (_camera != null)
        {
            _camera.Zoom = new Vector2(0.1f, 0.1f);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_ship != null && _camera != null)
        {
            _camera.Position = _ship.Position;
        }
    }
}
