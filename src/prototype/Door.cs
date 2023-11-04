using Godot;
using System;
using System.Threading.Tasks;

public class Door : Node2D
{
    [Export] private NodePath _animPlayerPath;

    private AnimationPlayer _animPlayer;

    public override void _Ready()
    {
        _animPlayer = GetNode<AnimationPlayer>(_animPlayerPath);
    }

    public async Task Open()
    {
        _animPlayer.Play("door_open");
        await ToSignal(_animPlayer, "animation_finished");
    }
    
    public async Task Close()
    {
        _animPlayer.Play("door_close");
        await ToSignal(_animPlayer, "animation_finished");
    }
}
