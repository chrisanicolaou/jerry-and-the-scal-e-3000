using Godot;
using System;

public class TutorialPrompt : Control
{
    [Export] private NodePath _textContainerPath;
    [Export] private NodePath _textPath;

    private Control _textContainer;
    private Label _text;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _textContainer = GetNode<Control>(_textContainerPath);
        _text = GetNode<Label>(_textPath);
        Hide();
    }

    public void ShowPrompt(string text, LayoutPreset layout = LayoutPreset.CenterBottom)
    {
        _text.Text = "";
        _textContainer.SetAnchorsPreset(layout, true);
        _text.Text = text;
        Show();
    }
}
