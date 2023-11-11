using Godot;
using System;

public class MainMenu : Control
{
    [Signal] public delegate void PlayRequested();

    [Export] private NodePath _playButtonPath;

    private Button _playButton;

    public override void _Ready()
    {
        _playButton = GetNode<Button>(_playButtonPath);
        _playButton.Connect("pressed", this, nameof(OnPlayButtonPressed));
    }

    private void OnPlayButtonPressed() => EmitSignal(nameof(PlayRequested));
}
