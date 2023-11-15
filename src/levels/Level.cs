using Godot;
using System;
using System.Threading.Tasks;
using GithubGameJam2023.ui.modal;
using Godot.Collections;
using Array = Godot.Collections.Array;

public class Level : Node2D
{
    [Signal] public delegate void LevelCompleted();
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
    [Export] private NodePath _gunPickupPath;
    [Export] private ModalOptions _gunPickupModalOpts;
    [Export(PropertyHint.Enum, "Small,Large")] private string _modalSizeAsStr;

    private ModalManager _modalManager;
    private ModalSize _modalSize;
    private Door _entranceDoor;
    private Door _exitDoor;
    private Player _player;
    private Key _key;
    private bool _keyFound;
    private LevelUI _levelUI;

    public override void _Ready()
    {
        _modalManager = GetNode<ModalManager>("/root/ModalManager");
        Enum.TryParse(_modalSizeAsStr, out _modalSize);
        _entranceDoor = GetNode<Door>(_entranceDoorPath);
        _exitDoor = GetNode<Door>(_exitDoorPath);
        _key = GetNode<Key>(_keyPath);
        _player = GetNode<Player>(_playerPath);
        _player.NumOfBullets = _numOfBullets;
        _levelUI = GetNode<LevelUI>(_levelUIPath);
        _player.Freeze = true;
        _player.Visible = false;

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
        _levelUI.Connect(nameof(LevelUI.RetryRequested), this, nameof(OnRetryRequested));
        
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
        _player.Freeze = false;
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
        EmitSignal(nameof(LevelCompleted));
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

    private void OnRetryRequested() => EmitSignal(nameof(LevelCompleted));

    private async void HandleGunPickup()
    {
        _player.PickupGun();
        GetTree().Paused = true;
        await _modalManager.ShowModal(_gunPickupModalOpts, _modalSize);
        GetTree().Paused = false;
    }

    private async void HandleAmmoPickup(AmmoPickup ammoPickup)
    {
        ammoPickup?.QueueFree();
        _levelUI.AddBonusBullet();
        _player.NumOfBullets++;
    }
}
