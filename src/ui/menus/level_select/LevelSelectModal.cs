using Godot;
using System;
using System.Threading.Tasks;
using Godot.Collections;
using Array = Godot.Collections.Array;

public class LevelSelectModal : Panel
{
    [Signal] public delegate void LevelSelected(LevelData levelData);

    [Signal] public delegate void PanelClosed();
    
    [Export] private float _scrollAnimationDuration = 0.4f;
    [Export] private Tween.TransitionType _transType = Tween.TransitionType.Cubic;
    [Export] private Tween.EaseType _easeType = Tween.EaseType.Out;
    [Export] private PackedScene _pageScene;
    [Export] private PackedScene _levelNodeScene;
    [Export] private NodePath _scrollContainerPath;
    [Export] private NodePath _pagesContainerPath;
    [Export] private NodePath _nextPageButtonPath;
    [Export] private NodePath _previousPageButtonPath;
    [Export] private int _scrollAmount = 410;

    private GameDataManager _gameDataManager;
    private ScrollContainer _scrollContainer;
    private Control _pagesContainer;
    private PageButton _nextPageButton;
    private PageButton _previousPageButton;
    private int _pageIndex;
    private int _numOfPages;

    public override void _Ready()
    {
        _gameDataManager = GetNode<GameDataManager>("/root/GameDataManager");
        _scrollContainer = GetNode<ScrollContainer>(_scrollContainerPath);
        _pagesContainer = GetNode<Control>(_pagesContainerPath);
        _nextPageButton = GetNode<PageButton>(_nextPageButtonPath);
        _previousPageButton = GetNode<PageButton>(_previousPageButtonPath);
        _nextPageButton.Connect("pressed", this, nameof(OnNextPageButtonPressed));
        _previousPageButton.Connect("pressed", this, nameof(OnPreviousPageButtonPressed));
        Connect("gui_input", this, nameof(OnInputEvent));
        SetupPages();
    }

    public void ShowModal()
    {
        _pageIndex = -1;
        NavigateToPage();
        Show();
        (_pagesContainer.GetChild(0).GetChild(0) as Control).GrabFocus();
    }

    private void OnInputEvent(InputEvent inputEvent)
    {
        // Shroud was clicked
        if ((inputEvent is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.ButtonIndex == 1))
        {
            Hide();
            EmitSignal(nameof(PanelClosed));
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is InputEventKey keyEvent && keyEvent.IsActionPressed("ui_cancel"))
        {
            GetTree().SetInputAsHandled();
            Hide();
            EmitSignal(nameof(PanelClosed));
        }
    }

    private void SetupPages()
    {
        var levelsData = _gameDataManager.AllLevelsCollection.Levels;
        var currentPage = new Control(); // Initializing to new control to silence rider "this has not been initialized" errors
        var previousFocusNode = new Control();
        for (var i = 0; i < levelsData.Count; i++)
        {
            if (i % 4 == 0)
            {
                if (i > 0)
                {
                    previousFocusNode.FocusNext = _nextPageButtonPath;
                }
                var newPage = _pageScene.Instance<Control>();
                _pagesContainer.AddChild(newPage);
                previousFocusNode = _previousPageButton;
                currentPage = newPage;
                _numOfPages++;
            }

            var newNode = _levelNodeScene.Instance<LevelSelectNode>();
            currentPage.AddChild(newNode);
            newNode.Initialize(levelsData[i], i, _gameDataManager.PlayerData.HighestUnlockedLevelIndex < i);
            newNode.Connect(nameof(LevelSelectNode.LevelNodeSelected), this, nameof(OnLevelNodeSelected), new Array { levelsData[i] });
            if (i > 0)
            {
                newNode.FocusPrevious = previousFocusNode.GetPath();
                previousFocusNode.FocusNext = newNode.GetPath();
            }
            previousFocusNode = newNode;
        }

        _scrollContainer.ScrollHorizontal = 0;
        UpdateButtons();
    }

    private void OnLevelNodeSelected(LevelData levelData)
    {
        EmitSignal(nameof(LevelSelected), levelData);
    }

    private void OnNextPageButtonPressed() => NavigateToPage();

    private void OnPreviousPageButtonPressed() => NavigateToPage(false);

    private async Task NavigateToPage(bool forwards = true)
    {
        _pageIndex += forwards ? 1 : -1;
        var targetScrollAmount = _scrollAmount * _pageIndex;
        var tween = GetTree().CreateTween().SetTrans(_transType).SetEase(_easeType);
        tween.TweenProperty(_scrollContainer, "scroll_horizontal", targetScrollAmount, _scrollAnimationDuration);
        UpdateButtons();
        (_pagesContainer.GetChild(_pageIndex).GetChild(0) as Control).GrabFocus();
    }

    private void UpdateButtons()
    {
        if (_pageIndex <= 0) _previousPageButton.Deactivate();
        else _previousPageButton.Activate();
        if (_pageIndex >= _numOfPages - 1) _nextPageButton.Deactivate();
        else _nextPageButton.Activate();
    }
}
