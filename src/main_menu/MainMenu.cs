using Godot;
using System;
using ChiciStudios.GithubGameJam2023.Common.Audio;

public class MainMenu : Node2D
{
    [Signal] public delegate void PlayRequested();

    [Signal] public delegate void LevelSelected(LevelData level);

    [Export] private NodePath _playButtonPath;
    [Export] private NodePath _levelSelectButtonPath;
    [Export] private NodePath _levelSelectModalPath;
    [Export] private NodePath _optionsButtonPath;
    [Export] private NodePath _optionsModalPath;
    [Export] private NodePath _uiExceptionFadeRectPath;
    [Export] private NodePath _rollingBoulderPath;
    [Export] private AudioStream _musicTrack;
    [Export] private Vector2 _rollingBoulderStartPosition = new Vector2(-209, 271);
    [Export] private Vector2 _rollingBoulderLinearVelocity = new Vector2(70, 0);
    [Export] private float _rollingBoulderAngularVelocity = 3;

    private Button _playButton;
    private Button _levelSelectButton;
    private Button _optionsButton;
    private ColorRect _uiExceptionFadeRect;
    private LevelSelectModal _levelSelectModal;
    private OptionsMenu _optionsModal;
    private AudioManager _audioManager;
    private ScalableItemV2 _rollingBoulder;

    public override void _Ready()
    {
        _uiExceptionFadeRect = GetNode<ColorRect>(_uiExceptionFadeRectPath);
        _uiExceptionFadeRect.Color = Colors.Black;
        _audioManager = GetNode<AudioManager>("/root/AudioManager");
        _levelSelectModal = GetNode<LevelSelectModal>(_levelSelectModalPath);
        _levelSelectModal.Connect(nameof(LevelSelectModal.LevelSelected), this, nameof(OnLevelSelected));
        _levelSelectModal.Connect(nameof(LevelSelectModal.PanelClosed), this, nameof(OnNavigateBackToMainMenu));
        _playButton = GetNode<Button>(_playButtonPath);
        _playButton.Connect("pressed", this, nameof(OnPlayButtonPressed));
        _levelSelectButton = GetNode<Button>(_levelSelectButtonPath);
        _levelSelectButton.Connect("pressed", this, nameof(OnLevelSelectButtonPressed));
        _rollingBoulder = GetNode<ScalableItemV2>(_rollingBoulderPath);
        _optionsButton = GetNode<Button>(_optionsButtonPath);
        _optionsButton.Connect("pressed", this, nameof(OnOptionsButtonPressed));
        _optionsModal = GetNode<OptionsMenu>(_optionsModalPath);
        // _playButton.CallDeferred("grab_focus");
    }

    public void Start()
    {
        _uiExceptionFadeRect.Color = Colors.Transparent;
        _audioManager.PlayMusic(_musicTrack);
        _rollingBoulderStartPosition = _rollingBoulderStartPosition;
        _rollingBoulder.LinearVelocity = _rollingBoulderLinearVelocity;
        _rollingBoulder.AngularVelocity = _rollingBoulderAngularVelocity;
        _rollingBoulder.ApplyImpulse(Vector2.Zero, Vector2.Zero);
        GetTree().CreateTimer(0.1f).Connect("timeout", this, nameof(StartRollingTheBoulderBecauseEvenThoughTheBoulderShouldBeInItsOriginalStateAfterThisSceneIsInstantiatedItIsSomeHowBeingAffectedByPriorInstantiationsOfThisSceneResultingInItHavingADifferentInitialVelocityToTheOneSetInTheInspector));
    }

    public async void FadeInStart()
    {
        var tweenDuration = 2f;
        var tween = GetTree().CreateTween();
        tween.TweenProperty(_uiExceptionFadeRect, "color:a", 0f, tweenDuration).From(1f);
        await ToSignal(GetTree().CreateTimer(tweenDuration / 2), "timeout");
        _audioManager.PlayMusic(_musicTrack);
        GetTree().CreateTimer(0.1f).Connect("timeout", this, nameof(StartRollingTheBoulderBecauseEvenThoughTheBoulderShouldBeInItsOriginalStateAfterThisSceneIsInstantiatedItIsSomeHowBeingAffectedByPriorInstantiationsOfThisSceneResultingInItHavingADifferentInitialVelocityToTheOneSetInTheInspector));
    }

    private void StartRollingTheBoulderBecauseEvenThoughTheBoulderShouldBeInItsOriginalStateAfterThisSceneIsInstantiatedItIsSomeHowBeingAffectedByPriorInstantiationsOfThisSceneResultingInItHavingADifferentInitialVelocityToTheOneSetInTheInspector()
    {
        _rollingBoulder.ContinuousMovement = true;
        _rollingBoulder.ContinuousMovementStartPosition = _rollingBoulderStartPosition;
        _rollingBoulder.ContinuousMovementLinearVelocity = _rollingBoulderLinearVelocity;
        _rollingBoulder.ContinuousMovementAngularVelocity = _rollingBoulderAngularVelocity;
    }

    private void OnPlayButtonPressed() => EmitSignal(nameof(PlayRequested));

    private void OnLevelSelectButtonPressed() => _levelSelectModal.ShowModal();

    private void OnLevelSelected(LevelData levelData) => EmitSignal(nameof(LevelSelected), levelData);
    private void OnOptionsButtonPressed() => _optionsModal.Show();

    private void OnNavigateBackToMainMenu()
    {
        // _playButton.GrabFocus();
    }
}
