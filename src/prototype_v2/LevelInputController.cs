using Godot;
using System;

public class LevelInputController : Node
{
    private InteractionManager _interactionManager;
    
    public override void _Ready()
    {
        _interactionManager = GetNode<InteractionManager>($"/root/{nameof(InteractionManager)}");
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("interact"))
        {
            _interactionManager.TriggerActiveInteraction();
        }
    }
}
