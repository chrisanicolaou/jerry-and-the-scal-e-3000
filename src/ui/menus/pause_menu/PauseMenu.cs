using Godot;
using System;

public class PauseMenu : Control
{
    [Signal] public delegate void ResumeRequested();
    [Signal] public delegate void RetryRequested();
    [Signal] public delegate void QuitToMenuRequested();

    [Export] private NodePath _resumeButtonPath;
    [Export] private NodePath _retryButtonPath;
    [Export] private NodePath _optionsButtonPath;
    [Export] private NodePath _quitToMenuButtonPath;
    [Export] private NodePath _optionsModalPath;

    private Button _resumeButton;
    private Button _retryButton;
    private Button _optionsButton;
    private Button _quitToMenuButton;
    private OptionsMenu _optionsModal;

    public override void _Ready()
    {
        _resumeButton = GetNode<Button>(_resumeButtonPath);
        _retryButton = GetNode<Button>(_retryButtonPath);
        _optionsButton = GetNode<Button>(_optionsButtonPath);
        _quitToMenuButton = GetNode<Button>(_quitToMenuButtonPath);

        _resumeButton.Connect("pressed", this, nameof(OnResumeButtonPressed));
        _retryButton.Connect("pressed", this, nameof(OnRetryButtonPressed));
        _optionsButton.Connect("pressed", this, nameof(OnOptionsButtonPressed));
        _quitToMenuButton.Connect("pressed", this, nameof(OnQuitToMenuButtonPressed));

        _optionsModal = GetNode<OptionsMenu>(_optionsModalPath);
    }

    public void ShowAndFocus()
    {
        Show();
        // _resumeButton.CallDeferred("grab_focus");
    }

    private void OnResumeButtonPressed()
    {
        EmitSignal(nameof(ResumeRequested));
    }

    private void OnRetryButtonPressed()
    {
        EmitSignal(nameof(RetryRequested));
    }

    private void OnOptionsButtonPressed()
    {
        _optionsModal.Show();
    }

    private void OnQuitToMenuButtonPressed()
    {
        EmitSignal(nameof(QuitToMenuRequested));
    }
}
