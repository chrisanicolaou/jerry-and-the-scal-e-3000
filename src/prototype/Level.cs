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
        var playerScale = 0.5f;
        var exitDoorDuration = 0.5f;
        
        _player.GlobalPosition = _entranceDoor.GlobalPosition;
        var playerOriginalAlpha = _player.Modulate.a;
        _player.Modulate = new Color(_player.Modulate.r, _player.Modulate.g, _player.Modulate.b, 0);
        var playerOriginalScale = _player.Scale;
        _player.Scale = new Vector2(playerScale, playerScale);
        await _entranceDoor.Open();
        var tween = GetTree().CreateTween().SetParallel();
        tween.TweenProperty(_player, "scale", playerOriginalScale, exitDoorDuration);
        tween.TweenProperty(_player, "modulate:a", playerOriginalAlpha, exitDoorDuration);
        await ToSignal(tween, "finished");
        await _entranceDoor.Close();
        _player.Freeze = false;
    }
}
