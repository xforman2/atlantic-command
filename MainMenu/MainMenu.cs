using Godot;
using System;

public partial class MainMenu : Control
{
    private VBoxContainer _mainMenu;
    private VBoxContainer _playSubMenu;

    public override void _Ready()
    {
        LoadOptions();
        _mainMenu = GetNode<VBoxContainer>("MainMenu");
        _playSubMenu = GetNode<VBoxContainer>("PlaySubMenu");

        Button resumeButton = _mainMenu.GetNode<Button>("Resume");
        Button startButton = _mainMenu.GetNode<Button>("Start");
        Button optionsButton = _mainMenu.GetNode<Button>("Options");
        Button quitButton = _mainMenu.GetNode<Button>("Quit");

        Button newGameButton = _playSubMenu.GetNode<Button>("NewGame");
        Button savedGameButton = _playSubMenu.GetNode<Button>("SavedGame");

        resumeButton.Pressed += OnResumePressed;
        startButton.Pressed += OnStartPressed;
        optionsButton.Pressed += OnOptionsPressed;
        quitButton.Pressed += OnQuitPressed;


        newGameButton.Pressed += OnNewGamePressed;
        savedGameButton.Pressed += OnSavedGamePressed;
        resumeButton.Visible = GameState.Instance.HasStartedGame;
        var hasSave = SaveSystem.HasSave();
        savedGameButton.Visible = hasSave;
        GD.Print(hasSave);
        if (hasSave)
        {
            _playSubMenu.Size = new Vector2I(500, 400);
        }

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

    private void OnResumePressed()
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

    private void OnStartPressed()
    {
        _mainMenu.Visible = false;
        _playSubMenu.Visible = true;
    }

    private void OnQuitPressed()
    {
        GetTree().Quit();

    }

    private void OnOptionsPressed()
    {
        GetTree().ChangeSceneToFile("res://MainMenu/Options.tscn");

    }


    private void OnNewGamePressed()
    {

        GameState.Instance.HasStartedGame = true;
        SaveSystem.DeleteSave();
        var ship = ShipManager.Instance.CurrentShip;
        if (ship is not null)
        {
            ShipManager.Instance.CurrentShip.QueueFree();
            ShipManager.Instance.CurrentShip = null;
        }
        GameState.Instance.LastOrigin = SceneOrigin.ShipBuilder;

        GetTree().ChangeSceneToFile("res://ShipBuilder/ShipBuilder.tscn");


    }

    private void OnSavedGamePressed()
    {

        var ship = ShipManager.Instance.CurrentShip;
        if (ship is not null)
        {
            ShipManager.Instance.CurrentShip.QueueFree();
            ShipManager.Instance.CurrentShip = null;
        }

        GameState.Instance.HasStartedGame = true;
        var scene = GD.Load<PackedScene>("Ship/PlayerShip.tscn");
        ship = scene.Instantiate<PlayerShip>();
        var loadedShip = SaveSystem.LoadShip();
        AddChild(ship);
        SaveSystem.LoadFromSave(ship, loadedShip);
        ShipManager.Instance.CurrentShip = ship;
        ShipManager.Instance.ReparentShip(ship);
        ShipManager.Instance.SetShipWorldPosition(loadedShip.Position.ToVector2I());
        ship.DisableCamera();
        ship.GoToDock();


        GetTree().ChangeSceneToFile("res://ShipBuilder/ShipBuilder.tscn");
    }
}
