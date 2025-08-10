using Godot;
using System;

public abstract partial class FloorTile : Node2D
{
    protected virtual int DefaultHP => 100;

    public int HP { get; private set; }
    public Vector2I Origin { get; set; }
    public BuildableStructure StructureOnTop { get; set; }
    private Label _hpLabel;


    public override void _Ready()
    {
        _hpLabel = GetNode<Label>("HpLabel");
        ShowHpLabel(false);
    }

    public void ShowHpLabel(bool show)
    {
        if (_hpLabel == null)
            return;

        if (show)
        {
            UpdateHpLabel();
        }
        _hpLabel.Visible = show;
    }

    private void UpdateHpLabel()
    {
        if (_hpLabel != null)
        {
            _hpLabel.Text = HP.ToString();
        }
    }

    public void Init(Vector2I position)
    {
        Position = position;
        Origin = position;
        HP = DefaultHP;
    }
    public void TakeDamage(int damage)
    {
        GD.Print($"{damage}");
        HP -= damage;

        if (HP <= 0)
        {
            OnDestroyed();
        }
        UpdateHpLabel();
    }

    protected virtual void OnDestroyed()
    {
        if (StructureOnTop != null)
        {
            GD.Print($"Destroying structure on top: {StructureOnTop.Name}");
            StructureOnTop.QueueFree();
        }

        QueueFree();
    }
}
