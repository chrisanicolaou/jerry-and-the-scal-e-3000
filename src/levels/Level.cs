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
    [Export] private NodePath _gunPickupPath;
    [Export] private ModalOptions _gunPickupModalOpts;
    [Export(PropertyHint.Enum, "Small,Large")] private string _modalSizeAsStr;
    [Export] private ModalOptions _tutorialModalOpts;
    [Export] private DialogPrompt _dialogPrompt;

    private DialogBoxCanvas _dialogBoxCanvas;
    private ModalManager _modalManager;
    private ModalSize _modalSize;
    private Door _entranceDoor;
    private Door _exitDoor;
    private Player _player;
    private Key _key;
    private bool _keyFound;
    private LevelInputController _inputController;
    private LevelUI _levelUI;

    public override void _Ready()
    {
        _modalManager = GetNode<ModalManager>("/root/ModalManager");
        // _dialogBoxCanvas = GetNode<DialogBoxCanvas>("/root/DialogBoxCanvas");
        Enum.TryParse(_modalSizeAsStr, out _modalSize);
        _entranceDoor = GetNode<Door>(_entranceDoorPath);
        _exitDoor = GetNode<Door>(_exitDoorPath);
        _key = GetNode<Key>(_keyPath);
        _player = GetNode<Player>(_playerPath);
        _player.NumOfBullets = _numOfBullets;
        _levelUI = GetNode<LevelUI>(_levelUIPath);
        _player.Freeze = true;
        _player.Visible = false;
        _inputController = GetNode<LevelInputController>(_inputControllerPath);
        _inputController.Connect(nameof(LevelInputController.PauseRequested), this, nameof(OnPauseRequested));

        _player.Connect(nameof(Player.ShotsFired), this, nameof(OnShotsFired));
        _player.Connect(nameof(Player.ItemForcePutdown), this, nameof(OnItemForcePutdown));
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

        if (_gunPickupPath != null)
        {
            var gunPickup = GetNode<GunPickup>(_gunPickupPath);
            gunPickup.Connect(nameof(GunPickup.GunPickupRequested), this, nameof(OnGunPickupRequested));
        }
        
        _levelUI.Initialise(1, _numOfBullets > 0 ? _numOfBullets : 0);
        _levelUI.Connect(nameof(LevelUI.ResumeRequested), this, nameof(OnResumeRequested));
        _levelUI.Connect(nameof(LevelUI.RetryRequested), this, nameof(OnRetryRequested));
        _levelUI.Connect(nameof(LevelUI.QuitToMenuRequested), this, nameof(OnQuitToMenuRequested));
        
        if (StartAutomatically)
        {
            StartLevel();
        }
    }

    public async void StartLevel()
    {
        _player.GlobalPosition = _entranceDoor.GlobalPosition;
        await _entranceDoor.Open();
        _player.Visible = true;
        await _player.EnterLevel(_entranceDoor);
        await _entranceDoor.Close();

        if (_tutorialModalOpts == null && _dialogPrompt == null) _player.Freeze = false;
        
        if (_tutorialModalOpts != null)
        {
            await ShowModal(_tutorialModalOpts, ModalSize.Large);
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
        _levelUI.AddCollectedKey();
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
        _player.Freeze = true;
        _player.GlobalPosition = _exitDoor.GlobalPosition;
        await _exitDoor.Open();
        await _player.ExitLevel(_exitDoor);
        await _exitDoor.Close();
        EmitSignal(nameof(LevelCompleted));
    }

    private void OnItemCarryRequested(ScalableItemV2 item)
    {
        if (_player.ItemCarry != null)
        {
            if (_player.ItemCarry == item)
            {
                _player.PutdownItem();
                AddChild(item);
                return;
            }
            var itemDropped = _player.PutdownItem();
            AddChild(itemDropped);
        }
        RemoveChild(item);
        _player.PickupItem(item);
    }

    private void OnPlayerFell()
    {
        EmitSignal(nameof(RetryRequested));
    }

    private void OnShotsFired()
    {
        _levelUI.RemoveBullet();
    }

    private void OnItemForcePutdown(ScalableItem item)
    {
        AddChild(item);
    }

    private void OnGunPickupRequested() => HandleGunPickup();
    private void OnAmmoPickupFound(AmmoPickup ammoPickup) => HandleAmmoPickup(ammoPickup);

    private async void HandleGunPickup()
    {
        _player.PickupGun();
        ShowModal(_gunPickupModalOpts, _modalSize);
    }

    private async void HandleAmmoPickup(AmmoPickup ammoPickup)
    {
        ammoPickup?.QueueFree();
        _levelUI.AddBonusBullet();
        _player.NumOfBullets++;
    }

    private void OnPauseRequested()
    {
        GetTree().Paused = true;
        _levelUI.OpenPauseMenu();
    }

    private void OnResumeRequested()
    {
        _levelUI.ClosePauseMenu();
        GetTree().Paused = false;
    }

    private void OnRetryRequested() => EmitSignal(nameof(RetryRequested));

    private void OnQuitToMenuRequested()
    {
        EmitSignal(nameof(QuitToMenuRequested));
    }

    private async Task ShowModal(ModalOptions opts, ModalSize size)
    {
        GetTree().Paused = true;
        await _modalManager.ShowModal(opts, size);
        GetTree().Paused = false;
        _player.Freeze = false;
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
            _player.Freeze = false;
        }
    }
}
