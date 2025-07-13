using Godot;
using System;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        Button startButton = GetNode<Button>("Start");

        startButton.Pressed += OnStartPressed;
    }

    private void OnStartPressed()
    {
        GD.Print("Start pressed!");
        GetTree().ChangeSceneToFile("res://game.tscn");
    }
}
