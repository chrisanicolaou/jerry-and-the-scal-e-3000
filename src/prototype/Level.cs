using Godot;
using System;

public class Level : Node2D
{
    [Signal] public delegate void LevelCompleted();
    [Export] public bool StartAutomatically;
    [Export] private NodePath _entranceDoorPath;
    [Export] private NodePath _exitDoorPath;
    [Export] private NodePath _keyPath;
    [Export] private NodePath _playerPath;

    private Door _entranceDoor;
    private Door _exitDoor;
    private Player _player;
    private Key _key;
    private bool _keyFound;

    public override void _Ready()
    {
        _entranceDoor = GetNode<Door>(_entranceDoorPath);
        _exitDoor = GetNode<Door>(_exitDoorPath);
        _key = GetNode<Key>(_keyPath);
        _player = GetNode<Player>(_playerPath);
        _player.Freeze = true;
        _player.Visible = false;
        if (StartAutomatically)
        {
            StartLevel();
        }

        _key.Connect(nameof(Key.KeyFound), this, nameof(OnKeyFound));
        _exitDoor.Connect(nameof(Door.PlayerReachedDoor), this, nameof(OnExitDoorReached));
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
}
