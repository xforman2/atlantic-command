using Godot;
using System;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        Button startButton = GetNode<Button>("VBoxContainer/Start");
        Button optionsButton = GetNode<Button>("VBoxContainer/Options");
        Button quitButton = GetNode<Button>("VBoxContainer/Quit");

        startButton.Pressed += OnStartPressed;
        optionsButton.Pressed += OnOptionsPressed;
        quitButton.Pressed += OnQuitPressed;

    }

    private void OnStartPressed()
    {
        GetTree().ChangeSceneToFile("res://Game.tscn");

    }

    private void OnQuitPressed()
    {
        GetTree().Quit();

    }

    private void OnOptionsPressed()
    {
        GetTree().ChangeSceneToFile("res://MainMenu/Options.tscn");

    }
}

