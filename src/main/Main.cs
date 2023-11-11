using Godot;
using System;
using System.Threading.Tasks;
using ChiciStudios.GithubGameJam2023.Common.SceneManagement;

public class Main : Node
{
    [Export] private PackedScene _mainMenuScene;
    [Export] private PackedScene _preLevelScene;
    [Export] private NodePath _sceneSwitcherPath;

    private RootSceneSwitcher _sceneSwitcher;
    private GameDataManager _gameDataManager;
    private Node _currentRootScene;

    public override void _Ready()
    {
        _sceneSwitcher = GetNode<RootSceneSwitcher>(_sceneSwitcherPath);
        _gameDataManager = GetNode<GameDataManager>("/root/GameDataManager");
        StartGame();
        // RestartPrototype();
        // CreatePrototypeLevel();
    }

    private async void StartGame()
    {
        var mainMenu = await SwitchRootScene<MainMenu>(_mainMenuScene, false);
        mainMenu.Connect(nameof(MainMenu.PlayRequested), this, nameof(OnPlayRequested));
    }

    private void OnPlayRequested()
    {
        LoadLevel(_gameDataManager.CurrentLevel, _gameDataManager.CurrentLevelIndex);
    }

    private async Task<T> SwitchRootScene<T>(PackedScene packedScene, bool fadeOut = true, bool fadeIn = true) where T : Node
    {
        var instance = packedScene.Instance<T>();
        if (instance is Level level) level.StartAutomatically = false;
        if (fadeOut) await _sceneSwitcher.Transition(SceneTransitionDirection.Out);
        _currentRootScene?.QueueFree();
        AddChild(instance);
        _currentRootScene = instance;
        if (fadeIn) await _sceneSwitcher.Transition(SceneTransitionDirection.In);
        return instance;
    }

    private async void LoadLevel(LevelData level, int levelIndex)
    {
        var preLevel = await SwitchRootScene<PreLevel>(_preLevelScene, fadeIn: false);
        preLevel.SetLevelLabels(levelIndex, level.DisplayName);
        await _sceneSwitcher.Transition(SceneTransitionDirection.In);
        await preLevel.HoldForDuration();
        var levelInstance = await SwitchRootScene<Level>(level.Scene);
        levelInstance.StartLevel();
        // levelInstance.Connect(nameof(Level.LevelCompleted), this, nameof(OnLevelComplete));
    }

    // private async void RestartPrototype()
    // {
    //     var level = CreatePrototypeLevel();
    //     await _sceneSwitcher.Transition(new SceneTransitionOptions
    //         { Direction = SceneTransitionDirection.Out, Duration = 0.5f, Transition = SceneTransitionType.Line });
    //     _currentLevel?.Free();
    //     AddChild(level);
    //     _currentLevel = level;
    //     await _sceneSwitcher.Transition(new SceneTransitionOptions
    //         { Direction = SceneTransitionDirection.In, Duration = 0.5f, Transition = SceneTransitionType.Line });
    //     level.StartLevel();
    // }

    // private Level CreatePrototypeLevel()
    // {
    //     var level = _prototypeLevelScene.Instance<Level>();
    //     level.StartAutomatically = false;
    //     level.Connect(nameof(Level.LevelCompleted), this, nameof(OnLevelComplete));
    //     return level;
    // }

    // private void OnLevelComplete() => RestartPrototype();
}
