using Godot;
using System;
using System.Threading.Tasks;
using ChiciStudios.GithubGameJam2023.Common.Dialog;
using GithubGameJam2023.ui.modal;
using Godot.Collections;
using Array = Godot.Collections.Array;

public class Level : Node2D
{
    [Signal] public delegate void LevelCompleted();
    [Signal] public delegate void QuitToMenuRequested();
    [Signal] public delegate void RetryRequested();
    [Export] public bool StartAutomatically;
    // Negative numbers (except -1 to avoid spamming through multiple clicks) are for infinite bullets
    [Export] private int _numOfBullets = -2;
    [Export] private NodePath _entranceDoorPath;
    [Export] private NodePath _exitDoorPath;
    [Export] private NodePath _keyPath;
    [Export] private NodePath _playerPath;
    [Export] private Array<NodePath> _carryableItemPaths;
    [Export] private Array<NodePath> _ammoPickupPaths;
    [Export] private Array<NodePath> _fallZonePaths;
    [Export] private NodePath _levelUIPath;
    [Export] private NodePath _inputControllerPath;
    [Export] private ModalOptions _tutorialModalOpts;
    [Export] private DialogPrompt _dialogPrompt;

    private DialogBoxCanvas _dialogBoxCanvas;
    private ModalManager _modalManager;
    private Door _exitDoor;
    private Key _key;
    private bool _keyFound;
    private LevelInputController _inputController;
    
    protected Door EntranceDoor { get; private set; }
    protected Player Player { get; private set; }
    protected LevelUI UI { get; private set; }

    public override void _Ready()
    {
        _modalManager = GetNode<ModalManager>("/root/ModalManager");
        // _dialogBoxCanvas = GetNode<DialogBoxCanvas>("/root/DialogBoxCanvas");
        EntranceDoor = GetNode<Door>(_entranceDoorPath);
        _exitDoor = GetNode<Door>(_exitDoorPath);
        _key = GetNode<Key>(_keyPath);
        Player = GetNode<Player>(_playerPath);
        Player.NumOfBullets = _numOfBullets;
        UI = GetNode<LevelUI>(_levelUIPath);
        Player.Freeze = true;
        Player.Visible = false;
        _inputController = GetNode<LevelInputController>(_inputControllerPath);
        _inputController.Connect(nameof(LevelInputController.PauseRequested), this, nameof(OnPauseRequested));

        Player.Connect(nameof(Player.ShotsFired), this, nameof(OnShotsFired));
        Player.Connect(nameof(Player.ItemForcePutdown), this, nameof(OnItemForcePutdown));
        _key.Connect(nameof(Key.KeyFound), this, nameof(OnKeyFound));
        _exitDoor.Connect(nameof(Door.PlayerReachedDoor), this, nameof(OnExitDoorReached));

        for (var i = 0; i < _carryableItemPaths?.Count; i++)
        {
            var item = GetNode<ScalableItemV2>(_carryableItemPaths[i]);
            item.Connect(nameof(ScalableItemV2.ItemInteractionRequested), this, nameof(OnItemCarryRequested));
        }
        for (var i = 0; i < _fallZonePaths?.Count; i++)
        {
            var fallZone = GetNode<FallZone>(_fallZonePaths[i]);
            fallZone.Connect(nameof(FallZone.PlayerFell), this, nameof(OnPlayerFell));
        }
        for (var i = 0; i < _ammoPickupPaths?.Count; i++)
        {
            var ammoPickup = GetNode<AmmoPickup>(_ammoPickupPaths[i]);
            ammoPickup.Connect(nameof(AmmoPickup.AmmoPickupFound), this, nameof(OnAmmoPickupFound), new Array { ammoPickup });
        }
        
        UI.Initialise(1, _numOfBullets > 0 ? _numOfBullets : 0);
        UI.Connect(nameof(LevelUI.ResumeRequested), this, nameof(OnResumeRequested));
        UI.Connect(nameof(LevelUI.RetryRequested), this, nameof(OnRetryRequested));
        UI.Connect(nameof(LevelUI.QuitToMenuRequested), this, nameof(OnQuitToMenuRequested));
        
        if (StartAutomatically)
        {
            StartLevel();
        }
    }

    public virtual async void StartLevel()
    {
        Player.GlobalPosition = EntranceDoor.GlobalPosition;
        await EntranceDoor.Open();
        Player.Visible = true;
        await Player.EnterLevel(EntranceDoor);
        await EntranceDoor.Close();

        if (_tutorialModalOpts == null && _dialogPrompt == null) Player.Freeze = false;
        
        if (_tutorialModalOpts != null)
        {
            await ShowModal(_tutorialModalOpts);
        }

        // if (_dialogPrompt != null)
        // {
        //     await ShowDialogPrompt(_dialogPrompt);
        // }
    }

    private async void OnKeyFound()
    {
        _keyFound = true;
        _key.QueueFree();
        UI.AddCollectedKey();
    }

    private async void OnExitDoorReached()
    {
        if (_keyFound)
        {
            await _exitDoor.Unlock();
            EndLevel();
        }
    }

    public async void EndLevel()
    {
        Player.Freeze = true;
        Player.GlobalPosition = _exitDoor.GlobalPosition;
        await _exitDoor.Open();
        await Player.ExitLevel(_exitDoor);
        await _exitDoor.Close();
        EmitSignal(nameof(LevelCompleted));
    }

    private void OnItemCarryRequested(ScalableItemV2 item)
    {
        if (Player.ItemCarry != null)
        {
            if (Player.ItemCarry == item)
            {
                Player.PutdownItem();
                AddChild(item);
                return;
            }
            var itemDropped = Player.PutdownItem();
            AddChild(itemDropped);
        }
        RemoveChild(item);
        Player.PickupItem(item);
    }

    private void OnPlayerFell()
    {
        EmitSignal(nameof(RetryRequested));
    }

    private void OnShotsFired()
    {
        UI.RemoveBullet();
    }

    private void OnItemForcePutdown(ScalableItem item)
    {
        AddChild(item);
    }
    private void OnAmmoPickupFound(AmmoPickup ammoPickup) => HandleAmmoPickup(ammoPickup);

    private async void HandleAmmoPickup(AmmoPickup ammoPickup)
    {
        ammoPickup?.QueueFree();
        UI.AddBonusBullet();
        Player.NumOfBullets++;
    }

    private void OnPauseRequested()
    {
        GetTree().Paused = true;
        UI.OpenPauseMenu();
    }

    private void OnResumeRequested()
    {
        UI.ClosePauseMenu();
        GetTree().Paused = false;
    }

    private void OnRetryRequested() => EmitSignal(nameof(RetryRequested));

    private void OnQuitToMenuRequested()
    {
        EmitSignal(nameof(QuitToMenuRequested));
    }

    protected async Task ShowModal(ModalOptions opts)
    {
        GetTree().Paused = true;
        await _modalManager.ShowModal(opts, ModalSize.Large);
        GetTree().Paused = false;
        Player.Freeze = false;
    }

    private async Task ShowDialogPrompt(DialogPrompt prompt)
    {
        _dialogBoxCanvas.DialogBox.Show(prompt);
        _dialogBoxCanvas.Connect(nameof(DialogBoxCanvas.AdvanceRequested), this, nameof(OnDialogAdvanceRequested));
    }

    private void OnDialogAdvanceRequested()
    {
        if (_dialogBoxCanvas.DialogBox.CanAdvance)
        {
            _dialogBoxCanvas.DialogBox.AdvancePrompt();
        }
        else
        {
            GetTree().Paused = false;
            Player.Freeze = false;
        }
    }
}
