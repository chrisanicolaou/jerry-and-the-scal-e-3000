using Godot;
using System;
using Godot.Collections;
using Array = Godot.Collections.Array;

public class Level : Node2D
{
    [Signal] public delegate void LevelCompleted();
    [Export] public bool StartAutomatically;
    [Export] private int _numOfBullets = 1;
    [Export] private NodePath _entranceDoorPath;
    [Export] private NodePath _exitDoorPath;
    [Export] private NodePath _keyPath;
    [Export] private NodePath _playerPath;
    [Export] private Array<NodePath> _coinPaths;
    [Export] private Array<NodePath> _carryableItemPaths;
    [Export] private NodePath _levelUIPath;

    private Door _entranceDoor;
    private Door _exitDoor;
    private Player _player;
    private Key _key;
    private bool _keyFound;
    private Coin[] _coins;
    private LevelUI _levelUI;
    private ScalableItem[] _carryableItems;

    public override void _Ready()
    {
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

        _coins = new Coin[_coinPaths.Count];
        for (var i = 0; i < _coinPaths.Count; i++)
        {
            _coins[i] = GetNode<Coin>(_coinPaths[i]);
            _coins[i].Connect(nameof(Coin.CoinFound), this, nameof(OnCoinFound), new Array(i));
        }

        _carryableItems = new ScalableItem[_carryableItemPaths.Count];
        for (var i = 0; i < _carryableItemPaths.Count; i++)
        {
            _carryableItems[i] = GetNode<ScalableItem>(_carryableItemPaths[i]);
            _carryableItems[i].Connect(nameof(ScalableItem.ItemInteractionRequested), this, nameof(OnItemCarryRequested));
        }
        
        _levelUI.Initialise(1, _coins.Length, _numOfBullets);
        
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

    private void OnCoinFound(int coinIndex)
    {
        _coins[coinIndex].QueueFree();
        _levelUI.AddCollectedCoin();
    }

    private void OnItemCarryRequested(ScalableItem item)
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

    private void OnShotsFired()
    {
        _levelUI.RemoveBullet();
    }

    private void OnItemForcePutdown(ScalableItem item)
    {
        AddChild(item);
    }
}
