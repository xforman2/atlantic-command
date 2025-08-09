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
        var ship = ShipManager.Instance.CurrentShip;
        GD.Print("LAST ORIGIN:", GameState.Instance.LastOrigin);

        if (GameState.Instance.LastOrigin == SceneOrigin.Game)
        {
            if (ship is not null)
            {
                ship.EnableCamera();
            }
            GetTree().ChangeSceneToFile("res://Game.tscn");
            return;
        }
        if (GameState.Instance.LastOrigin == SceneOrigin.ShipBuilder)
        {
            if (ship is not null)
            {
                ship.DisableCamera();
            }
            GetTree().ChangeSceneToFile("res://ShipBuilder/ShipBuilder.tscn");
            return;
        }

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
