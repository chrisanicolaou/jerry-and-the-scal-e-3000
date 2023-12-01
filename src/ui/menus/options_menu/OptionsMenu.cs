using Godot;
using System;
using ChiciStudios.GithubGameJam2023.Common.Audio;
using Godot.Collections;
using Array = Godot.Collections.Array;

public class OptionsMenu : Panel
{
    [Export] private NodePath _masterSliderPath;
    [Export] private NodePath _musicSliderPath;
    [Export] private NodePath _sfxSliderPath;
    [Export] private NodePath _aimLineTogglePath;
    [Export] private NodePath _easyModeTogglePath;
    [Export] private NodePath _restoreDefaultButtonPath;

    private Slider _masterSlider;
    private Slider _musicSlider;
    private Slider _sfxSlider;
    private CheckBox _aimLineToggle;
    private CheckBox _easyModeToggle;
    private Button _restoreDefaultButton;
    private PlayerData _playerData;
    private AudioManager _audioManager;
    
    // Hard coding default values directly into the OptionsMenu for the sake of simplicity
    private bool _aimLineDefault = true;
    private bool _easyModeDefault = false;
    private int _masterSliderDefault = 0;
    private int _musicSliderDefault = 0;
    private int _sfxSliderDefault = 0;

    public override void _Ready()
    {
        _playerData = GetNode<GameDataManager>("/root/GameDataManager").PlayerData;
        _audioManager = GetNode<AudioManager>("/root/AudioManager");
        _masterSlider = GetNode<Slider>(_masterSliderPath);
        _musicSlider = GetNode<Slider>(_musicSliderPath);
        _sfxSlider = GetNode<Slider>(_sfxSliderPath);
        _aimLineToggle = GetNode<CheckBox>(_aimLineTogglePath);
        _easyModeToggle = GetNode<CheckBox>(_easyModeTogglePath);
        _restoreDefaultButton = GetNode<Button>(_restoreDefaultButtonPath);

        _masterSlider.Connect("value_changed", this, nameof(OnSliderValueChanged), new Array { AudioBusName.Master });
        _musicSlider.Connect("value_changed", this, nameof(OnSliderValueChanged), new Array { AudioBusName.Music });
        _sfxSlider.Connect("value_changed", this, nameof(OnSliderValueChanged), new Array { AudioBusName.Sfx });
        _aimLineToggle.Connect("toggled", this, nameof(OnAimLineToggled));
        _easyModeToggle.Connect("toggled", this, nameof(OnEasyModeToggled));
        _restoreDefaultButton.Connect("pressed", this, nameof(RestoreDefaults));
        
        _masterSlider.Value = _playerData.MasterDb;
        _musicSlider.Value = _playerData.MusicDb;
        _sfxSlider.Value = _playerData.SfxDb;
        _aimLineToggle.SetPressedNoSignal(_playerData.AimLineEnabled);
        _easyModeToggle.SetPressedNoSignal(_playerData.EasyModeEnabled);
        
        Connect("gui_input", this, nameof(OnInputEvent));
        Hide();
    }

    private void OnInputEvent(InputEvent inputEvent)
    {
        // Shroud was clicked
        if (inputEvent is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.ButtonIndex == 1)
        {
            Hide();
        }
    }

    private void OnSliderValueChanged(float value, string busName)
    {
        var dbValue = GetDbValueFromSliderValue(value);
        switch (busName)
        {
            case AudioBusName.Master:
                _playerData.MasterDb = dbValue;
                break;
            case AudioBusName.Music:
                _playerData.MusicDb = dbValue;
                break;
            case AudioBusName.Sfx:
                _playerData.SfxDb = dbValue;
                break;
        }
        _audioManager.SetBusDb(busName, dbValue);
    }

    private void OnAimLineToggled(bool toggled)
    {
        _playerData.AimLineEnabled = toggled;
    }

    private void OnEasyModeToggled(bool toggled)
    {
        _playerData.EasyModeEnabled = toggled;
    }

    private void RestoreDefaults()
    {
        _aimLineToggle.Pressed = _aimLineDefault;
        _easyModeToggle.Pressed = _easyModeDefault;
        _masterSlider.Value = _masterSliderDefault;
        _musicSlider.Value = _musicSliderDefault;
        _sfxSlider.Value = _sfxSliderDefault;
    }

    private float GetDbValueFromSliderValue(float value)
    {
        // When at the end of the slider, set volume low enough to be effectively muted
        return Math.Abs(value - _masterSlider.MinValue) < 0.1 ? -80 : value;
    }
}
