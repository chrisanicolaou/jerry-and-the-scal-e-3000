using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ChiciStudios.GithubGameJam2023.Common.Audio;
using ChiciStudios.GithubGameJam2023.Common.SceneManagement;

public class Main : Node
{
    private const string PlayerDataSavePath = "user://player_data_ggo_2023.tres";
    [Export] private PackedScene _mainMenuScene;
    [Export] private PackedScene _preLevelScene;
    [Export] private PackedScene _thanksForPlayingScene;
    [Export] private NodePath _sceneSwitcherPath;
    [Export] private Resource _playerDataResourceScript;

    private RootSceneSwitcher _sceneSwitcher;
    private GameDataManager _gameDataManager;
    private AudioManager _audioManager;
    private Node _currentRootScene;
    private string _playerDataResourceScriptPath;

    public override void _Ready()
    {
        _sceneSwitcher = GetNode<RootSceneSwitcher>(_sceneSwitcherPath);
        _playerDataResourceScriptPath = _playerDataResourceScript.ResourcePath;
        _gameDataManager = GetNode<GameDataManager>("/root/GameDataManager");
        _gameDataManager.PlayerData = ResourceLoader.Load(PlayerDataSavePath) as PlayerData ?? GD.Load<CSharpScript>(_playerDataResourceScriptPath).New() as PlayerData;
        Debug.Assert(_gameDataManager.PlayerData != null, "_gameDataManager.PlayerData != null");
        
        _audioManager = GetNode<AudioManager>("/root/AudioManager");
        _audioManager.SetBusDb(AudioBusName.Master, _gameDataManager.PlayerData.MasterDb);
        _audioManager.SetBusDb(AudioBusName.Music, _gameDataManager.PlayerData.MusicDb);
        _audioManager.SetBusDb(AudioBusName.Sfx, _gameDataManager.PlayerData.SfxDb);
        StartGame();
        // RestartPrototype();
        // CreatePrototypeLevel();
    }

    private async void StartGame()
    {
        var mainMenu = _mainMenuScene.Instance<MainMenu>();
        GD.Print($"Instance ID: {mainMenu.GetInstanceId()}");
        _currentRootScene = mainMenu;
        AddChild(mainMenu);
        mainMenu.Connect(nameof(MainMenu.PlayRequested), this, nameof(OnPlayRequested));
        mainMenu.Connect(nameof(MainMenu.LevelSelected), this, nameof(OnLevelSelected));
        await _sceneSwitcher.Transition(new SceneTransitionOptions { Direction = SceneTransitionDirection.In });
        mainMenu.FadeInStart();
    }

    private async Task LoadMainMenu()
    {
        var mainMenu = await SwitchRootScene<MainMenu>(_mainMenuScene, fadeIn: false);
        GD.Print($"Instance ID: {mainMenu.GetInstanceId()}");
        mainMenu.Connect(nameof(MainMenu.PlayRequested), this, nameof(OnPlayRequested));
        mainMenu.Connect(nameof(MainMenu.LevelSelected), this, nameof(OnLevelSelected));
        mainMenu.Start();
        _sceneSwitcher.Transition(SceneTransitionDirection.In);
    }

    private void OnPlayRequested()
    {
        var level = _gameDataManager.GetLevelData(_gameDataManager.PlayerData.CurrentLevelIndex);
        if (level == null)
        {
            _gameDataManager.PlayerData.CurrentLevelIndex = 0;
            level = _gameDataManager.GetLevelData(_gameDataManager.PlayerData.CurrentLevelIndex);
        }
        LoadLevel(level, _gameDataManager.PlayerData.CurrentLevelIndex);
    }

    private void OnLevelSelected(LevelData levelData)
    {
        _gameDataManager.PlayerData.CurrentLevelIndex = _gameDataManager.AllLevelsCollection.Levels.IndexOf(levelData);
        LoadLevel(levelData, _gameDataManager.PlayerData.CurrentLevelIndex);
    }

    private async Task<T> SwitchRootScene<T>(PackedScene packedScene, bool fadeOut = true, bool fadeIn = true) where T : Node
    {
        var instance = packedScene.Instance<T>();
        if (instance is Level level) level.StartAutomatically = false;
        if (fadeOut) await _sceneSwitcher.Transition(SceneTransitionDirection.Out);
        RemoveChild(_currentRootScene);
        _currentRootScene?.QueueFree();
        AddChild(instance);
        _currentRootScene = instance;
        if (fadeIn) await _sceneSwitcher.Transition(SceneTransitionDirection.In);
        return instance;
    }

    private async Task LoadLevel(LevelData level, int levelIndex, bool playPreLevel = true)
    {
        if (playPreLevel)
        {
            var preLevel = await SwitchRootScene<PreLevel>(_preLevelScene, fadeIn: false);
            preLevel.SetLevelLabels(levelIndex, level.DisplayName);
            await _sceneSwitcher.Transition(SceneTransitionDirection.In);
            await preLevel.HoldForDuration();
        }
        GetTree().Paused = false;
        var levelInstance = await SwitchRootScene<Level>(level.Scene);
        levelInstance.Connect(nameof(Level.LevelCompleted), this, nameof(OnLevelComplete));
        levelInstance.Connect(nameof(Level.RetryRequested), this, nameof(RetryLevel));
        levelInstance.Connect(nameof(Level.QuitToMenuRequested), this, nameof(QuitToMenu));
        levelInstance.StartLevel();
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

    private void OnLevelComplete()
    {
        _gameDataManager.PlayerData.CurrentLevelIndex++;
        if (_gameDataManager.GetLevelData(_gameDataManager.PlayerData.CurrentLevelIndex) == null)
        {
            _gameDataManager.PlayerData.CurrentLevelIndex = 0;
            // LoadMainMenu();
            LoadThanksForPlaying();
        }
        else
        {
            if (_gameDataManager.PlayerData.CurrentLevelIndex > _gameDataManager.PlayerData.HighestUnlockedLevelIndex) _gameDataManager.PlayerData.HighestUnlockedLevelIndex++;
            LoadLevel(_gameDataManager.GetLevelData(_gameDataManager.PlayerData.CurrentLevelIndex), _gameDataManager.PlayerData.CurrentLevelIndex);
        }

        SavePlayerData();
    }

    private async void LoadThanksForPlaying()
    {
        var thanksForPlaying = await SwitchRootScene<ThanksForPlaying>(_thanksForPlayingScene);
        thanksForPlaying.ScrollCredits();
        thanksForPlaying.Connect(nameof(ThanksForPlaying.CreditsFinished), this, nameof(QuitToMenu));
    }

    private async void RetryLevel()
    {
        await LoadLevel(_gameDataManager.GetLevelData(_gameDataManager.PlayerData.CurrentLevelIndex), _gameDataManager.PlayerData.CurrentLevelIndex, false);
        GetTree().Paused = false;
    }

    private async void QuitToMenu()
    {
        await LoadMainMenu();
        GetTree().Paused = false;
    }

    private void SavePlayerData()
    {
        if (!OS.IsUserfsPersistent())
        {
            GD.PrintErr("Can't save! Make sure your browser cookies are enabled & that you aren't private browsing.");
            return;
        }

        ResourceSaver.Save(PlayerDataSavePath, _gameDataManager.PlayerData);
    }

    public override void _Notification(int what)
    {
        // In itch.io, clicking the game brings it in to focus. We want to stop this input event from propagating to prevent the user from shooting or clicking things they don't want to
        if (what == NotificationWmFocusIn)
        {
            GD.Print("Focusing...");
            GetTree().SetInputAsHandled();
        }
    }
}
