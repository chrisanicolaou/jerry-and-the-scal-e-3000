using Godot;
using System;

public class Coin : Node2D
{
    [Signal] public delegate void CoinFound();
    [Export] private NodePath _interactionAreaPath;
    [Export] private NodePath _animPlayerPath;

    public override void _Ready()
    {
        var interactionArea = GetNode<InteractionArea>(_interactionAreaPath);
        interactionArea.Connect(nameof(InteractionArea.InteractionAreaActivated), this, nameof(OnInteractionAreaActivated));
        GetNode<AnimationPlayer>(_animPlayerPath).Play("rotate");
    }

    public virtual void OnInteractionAreaActivated()
    {
        EmitSignal(nameof(CoinFound));
    }
}
