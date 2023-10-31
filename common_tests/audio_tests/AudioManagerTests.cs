using Godot;
using System;
using ChiciStudios.GithubGameJam2023.Common.Audio;
using Godot.Collections;

public class AudioManagerTests : Node
{
    [Export] private Array<AudioStream> _music;
    [Export] private Array<AudioStream> _sfx;
    [Export] private NodePath _playMusicPath;
    [Export] private NodePath _stopMusicPath;
    [Export] private NodePath _playSfxPath;
    private AudioManager _audioManager;
    private Button _playMusicButton;
    private Button _stopMusicButton;
    private Button _playSfxButton;

    public override void _Ready()
    {
        _audioManager = GetNode<AudioManager>("AudioManager");
        _playMusicButton = GetNode<Button>(_playMusicPath);
        _stopMusicButton = GetNode<Button>(_stopMusicPath);
        _playSfxButton = GetNode<Button>(_playSfxPath);

        _playMusicButton.Connect("pressed", this, nameof(OnPlayMusicPressed));
        _stopMusicButton.Connect("pressed", this, nameof(OnStopMusicPressed));
        _playSfxButton.Connect("pressed", this, nameof(OnPlaySfxPressed));
    }

    private void OnPlayMusicPressed()
    {
        var track = _music[RandomNumberFromRange(_music.Count)];
        _audioManager.PlayMusic(track);
    }

    private void OnStopMusicPressed()
    {
        _audioManager.StopMusic();
    }

    private void OnPlaySfxPressed()
    {
        var track = _sfx[RandomNumberFromRange(_sfx.Count)];
        _audioManager.PlaySfx(track);
    }

    private int RandomNumberFromRange(int end)
    {
        return (int)(GD.Randi() % end);
    }
}
