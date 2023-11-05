using Godot;
using System;

public class Level : Node2D
{
    [Export] private bool _startAutomatically;
    [Export] private NodePath _entranceDoorPath;
    [Export] private NodePath _keyPath;
    [Export] private NodePath _playerPath;

    private Door _entranceDoor;
    private Player _player;
    private Key _key;

    public override void _Ready()
    {
        _entranceDoor = GetNode<Door>(_entranceDoorPath);
        _key = GetNode<Key>(_keyPath);
        _player = GetNode<Player>(_playerPath);
        _player.Freeze = true;
        if (_startAutomatically)
        {
            StartLevel();
        }

        _key.Connect(nameof(Key.KeyFound), this, nameof(OnKeyFound));
    }

    public async void StartLevel()
    {
        await _entranceDoor.Open();
        await _player.EnterLevel(_entranceDoor);
        await _entranceDoor.Close();
        _player.Freeze = false;
    }

    private async void OnKeyFound()
    {
        _key.QueueFree();
    }
}
