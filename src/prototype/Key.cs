using Godot;
using System;

public class Key : Node2D
{
    [Signal] public delegate void KeyFound();
    [Export] private NodePath _interactionAreaPath;
    protected InteractionArea InteractionArea { get; private set; }

    public override void _Ready()
    {
        InteractionArea = GetNode<InteractionArea>(_interactionAreaPath);
        InteractionArea.Connect(nameof(InteractionArea.InteractionAreaActivated), this, nameof(OnInteractionAreaActivated));
    }

    public virtual void OnInteractionAreaActivated()
    {
        EmitSignal(nameof(KeyFound));
    }
}
