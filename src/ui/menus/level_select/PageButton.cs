using Godot;
using System;

public class PageButton : TextureButton
{
    [Export] private Color _hoverColor;
    [Export] private Color _pressedColor;

    private Color _defaultColor;

    public override void _Ready()
    {
        _defaultColor = Modulate;
        Connect("mouse_entered", this, nameof(OnMouseEntered));
        Connect("mouse_exited", this, nameof(OnMouseExited));
        Connect("focus_entered", this, nameof(OnMouseEntered));
        Connect("focus_exited", this, nameof(OnMouseExited));
        Connect("button_down", this, nameof(OnButtonDown));
        Connect("button_up", this, nameof(OnButtonUp));
    }

    public void Deactivate()
    {
        Hide();
        Modulate = _defaultColor;
    }

    public void Activate()
    {
        Show();
        Modulate = _defaultColor;
    }

    private void OnMouseEntered()
    {
        Modulate = _hoverColor;
    }

    private void OnMouseExited()
    {
        Modulate = _defaultColor;
    }

    private void OnButtonDown()
    {
        Modulate = _pressedColor;
    }

    private void OnButtonUp()
    {
        Modulate = _hoverColor;
    }
}
