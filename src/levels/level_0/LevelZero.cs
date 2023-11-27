using Godot;
using System;

public class LevelZero : Level
{
    [Export] private NodePath _playerWakeUpAnimPlayerPath;
    [Export] private NodePath _playerWakeUpSpritePath;
    [Export] private NodePath _playerBlanketSpritePath;
    [Export] private NodePath _playerSpritePath;
    [Export] private ModalOptions _jerryModalOpts;
    [Export] private PackedScene _tutorialPromptScene;

    private AnimationPlayer _playerWakeUpAnimPlayer;
    private Sprite _playerWakeUpSprite;
    private Sprite _playerSprite;
    private Sprite _playerBlanketSprite;

    private string _tutorialPrompt = "Press A or D to move. Press W or space to jump";

    public override void _Ready()
    {
        base._Ready();
        _playerWakeUpAnimPlayer = GetNode<AnimationPlayer>(_playerWakeUpAnimPlayerPath);
        _playerWakeUpSprite = GetNode<Sprite>(_playerWakeUpSpritePath);
        _playerSprite = GetNode<Sprite>(_playerSpritePath);
        _playerBlanketSprite = GetNode<Sprite>(_playerBlanketSpritePath);
        _playerSprite.Hide();
        Player.Visible = true;
        UI.HideBottomBar();
    }

    public override async void StartLevel()
    {
        _playerWakeUpAnimPlayer.Play("wake_up");
        await ToSignal(_playerWakeUpAnimPlayer, "animation_finished");
        _playerWakeUpSprite.Hide();
        _playerSprite.Show();
        _playerBlanketSprite.Hide();
        await ShowModal(_jerryModalOpts);
        Player.Freeze = false;
        var tutorialPrompt = _tutorialPromptScene.Instance<TutorialPrompt>();
        UI.AddChild(tutorialPrompt);
        tutorialPrompt.ShowPrompt(_tutorialPrompt);
    }

    protected override async void OnExitDoorReached()
    {
        EndLevel();
    }
}
