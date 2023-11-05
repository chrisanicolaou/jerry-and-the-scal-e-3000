using Godot;
using System;

public class Level : Node2D
{
    [Export] private bool _startAutomatically;
    [Export] private NodePath _entranceDoorPath;
    [Export] private NodePath _playerPath;

    private Door _entranceDoor;
    private Player _player;

    public override void _Ready()
    {
        _entranceDoor = GetNode<Door>(_entranceDoorPath);
        _player = GetNode<Player>(_playerPath);
        _player.Freeze = true;
        if (_startAutomatically)
        {
            StartLevel();
        }
    }

    public async void StartLevel()
    {
        await _entranceDoor.Open();
        await _player.EnterLevel(_entranceDoor);
        await _entranceDoor.Close();
        _player.Freeze = false;
    }
}
