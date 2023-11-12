using Godot;
using System;

public class GunPickup : Node2D
{
    [Signal] public delegate void GunPickupRequested();

    [Export] private NodePath _interactionAreaPath;

    public override void _Ready()
    {
        var interactionArea = GetNode<InteractionArea>(_interactionAreaPath);
        interactionArea.Connect(nameof(InteractionArea.InteractionAreaTriggered), this, nameof(OnInteractionAreaTriggered));
    }

    private void OnInteractionAreaTriggered()
    {
        EmitSignal(nameof(GunPickupRequested));
        QueueFree();
    }
}
