using Godot;
using System;

/// <summary>
/// INTERACTION SYSTEM:
/// - InteractionArea's that, when entered, will be added to a list (somewhere) of active interaction areas
/// - If an InteractionArea becomes the active area, it will emit a signal. This allows an InteractionArea's parent node control
///   over what happens when an interaction is valid. For example, an NPC might want to show a speech bubble, whereas a chest might glow & open slightly.
/// RANDOM NOTES:
/// - set global mask layer in InteractionManager so that in future if the player collision changes, i don't need to change each area
/// - how will the InteractionManager know which interaction is closest? Will have to know player pos - but then it can't be a singleton?
/// </summary>
public class InteractionArea : Area2D
{
    [Signal] public delegate void InteractionAreaTriggered();
    [Signal] public delegate void InteractionAreaActivated();
    [Signal] public delegate void InteractionAreaDeactivated();

    private InteractionManager _interactionManager;

    public override void _Ready()
    {
        Connect("area_entered", this, nameof(OnAreaEntered));
        Connect("area_exited", this, nameof(OnAreaExited));
        _interactionManager = GetNode<InteractionManager>($"/root/{nameof(InteractionManager)}");
        CollisionMask = _interactionManager.InteractionCollisionMask;
    }

    public void Trigger()
    {
        EmitSignal(nameof(InteractionAreaTriggered));
    }

    public void Activate()
    {
        EmitSignal(nameof(InteractionAreaActivated));
    }

    public void Deactivate()
    {
        EmitSignal(nameof(InteractionAreaDeactivated));
    }

    private void OnAreaEntered(Area2D area)
    {
        _interactionManager.RegisterNearbyInteractionArea(this);
    }

    private void OnAreaExited(Area2D area)
    {
        _interactionManager.DeregisterNearbyInteractionArea(this);
    }

    public override void _ExitTree()
    {
        _interactionManager.DeregisterNearbyInteractionArea(this);
    }
}
