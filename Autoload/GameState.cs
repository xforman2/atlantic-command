using Godot;
using System;

public enum SceneOrigin
{
    Game,
    ShipBuilder
}

public partial class GameState : Node
{
    public static GameState Instance { get; private set; }

    public SceneOrigin LastOrigin { get; set; } = SceneOrigin.ShipBuilder;

    public override void _Ready()
    {
        Instance = this;
    }
}
