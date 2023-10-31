using Godot;
using System;
using System.Threading.Tasks;
using ChiciStudios.GithubGameJam2023.Common.SceneManagement;
using Godot.Collections;

public class RootSceneSwitcherTests : Node
{
    [Export] private NodePath _sceneSwitcherPath;
    [Export] private NodePath _transitionButtonPath;
    [Export] private NodePath _dropdownButtonPath;
    [Export] private NodePath _colorDropdownButtonPath;
    [Export] private Dictionary<string, Texture> _colors;

    private RootSceneSwitcher _sceneSwitcher;
    private Button _transitionButton;
    private OptionButton _dropdownButton;
    private OptionButton _colorDropdownButton;

    public override void _Ready()
    {
        _sceneSwitcher = GetNode<RootSceneSwitcher>(_sceneSwitcherPath);
        _transitionButton = GetNode<Button>(_transitionButtonPath);
        _dropdownButton = GetNode<OptionButton>(_dropdownButtonPath);
        _colorDropdownButton = GetNode<OptionButton>(_colorDropdownButtonPath);
        _transitionButton.Connect("pressed", this, nameof(OnTransitionButtonPressed));
    }

    private async void OnTransitionButtonPressed()
    {
        Enum.TryParse<SceneTransitionType>(_dropdownButton.Text, out var transitionType);
        await _sceneSwitcher.Transition(new SceneTransitionOptions {Transition = transitionType, Duration = 0.5f, Direction = SceneTransitionDirection.Out, TransitionTexture = _colors[_colorDropdownButton.Text]});
        await _sceneSwitcher.Transition(new SceneTransitionOptions {Transition = transitionType, Duration = 0.5f, Direction = SceneTransitionDirection.In, TransitionTexture = _colors[_colorDropdownButton.Text]});
    }
}
