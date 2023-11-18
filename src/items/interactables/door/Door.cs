using Godot;
using System;
using System.Threading.Tasks;

public class Door : Node2D
{
    [Signal] public delegate void PlayerReachedDoor();
    [Export] private NodePath _interactionAreaPath;
    [Export] private NodePath _animPlayerPath;
    [Export] private NodePath _spritePath;
    [Export] private bool _isLocked;

    private AnimationPlayer _animPlayer;
    private Sprite _sprite;

    public override void _Ready()
    {
        _animPlayer = GetNode<AnimationPlayer>(_animPlayerPath);
        _sprite = GetNode<Sprite>(_spritePath);
        if (_isLocked) _sprite.Frame = _isLocked ? 0 : 4;
        InteractionArea = GetNode<InteractionArea>(_interactionAreaPath);
        InteractionArea.Connect(nameof(InteractionArea.InteractionAreaActivated), this, nameof(OnInteractionAreaActivated));
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
    protected InteractionArea InteractionArea { get; private set; }

    public virtual void OnInteractionAreaActivated()
    {
        EmitSignal(nameof(PlayerReachedDoor));
    }

    public async Task Unlock()
    {
        _isLocked = false;
        _animPlayer.Play("door_unlock");
        await ToSignal(_animPlayer, "animation_finished");
    }
}
