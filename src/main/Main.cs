using Godot;
using System;
using ChiciStudios.GithubGameJam2023.Common.SceneManagement;

public class Main : Node
{
    [Export] private PackedScene _prototypeLevelScene;
    [Export] private NodePath _sceneSwitcherPath;

    private RootSceneSwitcher _sceneSwitcher;
    private Level _currentLevel;

    public override void _Ready()
    {
        _sceneSwitcher = GetNode<RootSceneSwitcher>(_sceneSwitcherPath);
        RestartPrototype();
        CreatePrototypeLevel();
    }

    private async void RestartPrototype()
    {
        var level = CreatePrototypeLevel();
        await _sceneSwitcher.Transition(new SceneTransitionOptions
            { Direction = SceneTransitionDirection.Out, Duration = 0.5f, Transition = SceneTransitionType.Line });
        _currentLevel?.Free();
        AddChild(level);
        _currentLevel = level;
        await _sceneSwitcher.Transition(new SceneTransitionOptions
            { Direction = SceneTransitionDirection.In, Duration = 0.5f, Transition = SceneTransitionType.Line });
        level.StartLevel();
    }

    private Level CreatePrototypeLevel()
    {
        var level = _prototypeLevelScene.Instance<Level>();
        level.StartAutomatically = false;
        level.Connect(nameof(Level.LevelCompleted), this, nameof(OnLevelComplete));
        return level;
    }

    private void OnLevelComplete() => RestartPrototype();
}
