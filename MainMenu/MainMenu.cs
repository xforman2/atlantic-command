using Godot;
using System;

public partial class MainMenu : Control
{
    public override void _Ready()
    {
        LoadOptions();
        Button startButton = GetNode<Button>("VBoxContainer/Start");
        Button optionsButton = GetNode<Button>("VBoxContainer/Options");
        Button quitButton = GetNode<Button>("VBoxContainer/Quit");


        startButton.Pressed += OnStartPressed;
        optionsButton.Pressed += OnOptionsPressed;
        quitButton.Pressed += OnQuitPressed;

    }

    private void LoadOptions()
    {
        var config = new ConfigFile();
        var err = config.Load(Globals.ConfigPath);
        if (err != Error.Ok)
        {
            GD.Print("No config file found, using defaults.");
            return;
        }

        Globals.MusicVolume = (float)config.GetValue("Audio", "MusicVolume", Globals.MusicVolume);
        Globals.SfxVolume = (float)config.GetValue("Audio", "SfxVolume", Globals.SfxVolume);
        Globals.FPS = (int)config.GetValue("Game", "FPS", Globals.FPS);

        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), Globals.LinearToDb(Globals.MusicVolume));
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), Globals.LinearToDb(Globals.SfxVolume));
        Engine.MaxFps = Globals.FPS > 0 ? Globals.FPS : int.MaxValue;
    }

    private void OnStartPressed()
    {
        var ship = ShipManager.Instance.CurrentShip;

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
