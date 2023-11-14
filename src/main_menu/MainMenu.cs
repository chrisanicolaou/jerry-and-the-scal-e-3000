using Godot;
using System;
using ChiciStudios.GithubGameJam2023.Common.Audio;

public class MainMenu : Control
{
    [Signal] public delegate void PlayRequested();

    [Signal] public delegate void LevelSelected(LevelData level);

    [Export] private NodePath _playButtonPath;
    [Export] private NodePath _levelSelectButtonPath;
    [Export] private NodePath _levelSelectModalPath;
    [Export] private AudioStream _musicTrack;

    private Button _playButton;
    private Button _levelSelectButton;
    private LevelSelectModal _levelSelectModal;
    private AudioManager _audioManager;

    public override void _Ready()
    {
        _audioManager = GetNode<AudioManager>("/root/AudioManager");
        _audioManager.PlayMusic(_musicTrack);
        _levelSelectModal = GetNode<LevelSelectModal>(_levelSelectModalPath);
        _levelSelectModal.Connect(nameof(LevelSelectModal.LevelSelected), this, nameof(OnLevelSelected));
        _playButton = GetNode<Button>(_playButtonPath);
        _playButton.Connect("pressed", this, nameof(OnPlayButtonPressed));
        _levelSelectButton = GetNode<Button>(_levelSelectButtonPath);
        _levelSelectButton.Connect("pressed", this, nameof(OnLevelSelectButtonPressed));
    }

    private void OnPlayButtonPressed() => EmitSignal(nameof(PlayRequested));

    private void OnLevelSelectButtonPressed() => _levelSelectModal.ShowModal();

    private void OnLevelSelected(LevelData levelData) => EmitSignal(nameof(LevelSelected), levelData);
}
