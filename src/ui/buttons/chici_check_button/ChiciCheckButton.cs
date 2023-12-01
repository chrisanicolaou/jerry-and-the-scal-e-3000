using Godot;
using System;
using ChiciStudios.GithubGameJam2023.Common.Audio;
using ChiciStudios.GithubGameJam2023.Src.UI;

/// <summary>
/// I think Godot forces you to create new classes for each UI button you want to extend, since there's no way to inherit base button?
/// Not sure - but this is a complete copy paste of <see cref="ChiciButton"/>.
/// </summary>
public class ChiciCheckButton : CheckBox
{
    [Export] private MouseCursor _defaultCursor;
    [Export] private MouseCursor _hoverCursor;
    [Export] private AudioStream _hoverSfx;
    [Export] private AudioStream _pressedSfx;

    private AudioManager _audioManager;

    public override void _Ready()
    {
        _audioManager = GetNode<AudioManager>("/root/AudioManager");
        Connect("mouse_entered", this, nameof(OnMouseEntered));
        Connect("mouse_exited", this, nameof(OnMouseExited));
    }

    private void OnMouseEntered()
    {
        SetCursor(_hoverCursor);
        if (_hoverSfx != null) _audioManager.PlaySfx(_hoverSfx);
    }

    private void OnMouseExited()
    {
        SetCursor(_defaultCursor);
    }

    public override void _Pressed()
    {
        if (_pressedSfx != null) _audioManager.PlaySfx(_pressedSfx);
    }

    public override void _ExitTree()
    {
        SetCursor(_defaultCursor);
    }

    private void SetCursor(MouseCursor cursor)
    {
        if (cursor != null) Input.SetCustomMouseCursor(cursor.Cursor, cursor.Shape, cursor.Hotspot);
    }
}
