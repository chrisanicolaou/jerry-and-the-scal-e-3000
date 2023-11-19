using Godot;
using System;
using ChiciStudios.GithubGameJam2023.Common.Dialog;

public class DialogBoxCanvas : Control
{
    [Signal] public delegate void AdvanceRequested();
    [Export] private NodePath _dialogBoxPath;
    
    public DialogBox DialogBox { get; private set; }

    public override void _Ready()
    {
        DialogBox = GetNode<DialogBox>(_dialogBoxPath);
        Connect("gui_input", this, nameof(OnInputEvent));
    }

    private void OnInputEvent(InputEvent inputEvent)
    {
        // Shroud was clicked
        if ((inputEvent is InputEventMouseButton mouseButtonEvent && mouseButtonEvent.ButtonIndex == 1))
        {
            EmitSignal(nameof(AdvanceRequested));
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (inputEvent is InputEventKey keyEvent && keyEvent.IsActionPressed("ui_cancel"))
        {
            GetTree().SetInputAsHandled();
            EmitSignal(nameof(AdvanceRequested));
        }
    }
}
