using Godot;
using System;

public class LevelInputController : Node
{
    [Signal] public delegate void PauseRequested();
    private InteractionManager _interactionManager;
    
    public override void _Ready()
    {
        _interactionManager = GetNode<InteractionManager>($"/root/{nameof(InteractionManager)}");
    }

    public override void _Input(InputEvent input)
    {
        var handled = HandleUIInput(input);
        if (handled)
        {
            GetTree().SetInputAsHandled();
            return;
        }

        handled = HandleOtherInput(input);
        if (!handled) return;
        
        GetTree().SetInputAsHandled();
        return;
    }

    private bool HandleUIInput(InputEvent input)
    {
        if (input.IsActionPressed("ui_cancel"))
        {
            EmitSignal(nameof(PauseRequested));
            return true;
        }

        return false;
    }

    private bool HandleOtherInput(InputEvent input)
    {
        if (input.IsActionPressed("interact"))
        {
            _interactionManager.TriggerActiveInteraction();
            return true;
        }

        return false;
    }
}
