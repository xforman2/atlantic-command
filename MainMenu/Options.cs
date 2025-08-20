using Godot;
using System;

public partial class Options : Node
{
    private HSlider _musicSlider;
    private HSlider _sfxSlider;
    private OptionButton _fpsDropdown;
    private OptionButton _resDropdown;

    private ConfigFile _config;

    public override void _Ready()
    {
        _musicSlider = GetNode<HSlider>("OptionsContainer/MusicSlider");
        _sfxSlider = GetNode<HSlider>("OptionsContainer/SFXSlider");
        _fpsDropdown = GetNode<OptionButton>("OptionsContainer/FPSOptions");

        _musicSlider.Value = Globals.MusicVolume;
        _sfxSlider.Value = Globals.SfxVolume;

        _fpsDropdown.Clear();
        _fpsDropdown.AddItem("30 FPS", 30);
        _fpsDropdown.AddItem("60 FPS", 60);
        _fpsDropdown.AddItem("Uncapped", 0);

        for (int i = 0; i < _fpsDropdown.GetItemCount(); i++)
        {
            if (_fpsDropdown.GetItemId(i) == Globals.FPS)
            {
                _fpsDropdown.Select(i);
                break;
            }
        }


        _musicSlider.ValueChanged += v => SetMusicVolume((float)v);
        _sfxSlider.ValueChanged += v => SetSfxVolume((float)v);
        _fpsDropdown.ItemSelected += i =>
        {
            int fps = _fpsDropdown.GetItemId((int)i);
            SetFPS(fps);
        };
    }

    public void SetMusicVolume(float volume)
    {
        Globals.MusicVolume = volume;
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("Music"), LinearToDb(volume));
        SaveOptions();
    }

    public void SetSfxVolume(float volume)
    {
        Globals.SfxVolume = volume;
        AudioServer.SetBusVolumeDb(AudioServer.GetBusIndex("SFX"), LinearToDb(volume));
        SaveOptions();
    }

    public void SetFPS(int fps)
    {
        Globals.FPS = fps;
        Engine.MaxFps = fps > 0 ? fps : int.MaxValue;
        SaveOptions();
    }


    private void SaveOptions()
    {
        if (_config == null)
            _config = new ConfigFile();

        _config.SetValue("Audio", "MusicVolume", Globals.MusicVolume);
        _config.SetValue("Audio", "SfxVolume", Globals.SfxVolume);
        _config.SetValue("Game", "FPS", Globals.FPS);

        var err = _config.Save(Globals.ConfigPath);
        if (err != Error.Ok)
            GD.PrintErr("Failed to save options config: ", err);
    }

}
