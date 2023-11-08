using Godot;
using System;

public class ControlWrappedSprite : Control
{
    [Export] private NodePath _spritePath;

    public Sprite Sprite { get; private set; }

    public override void _Ready()
    {
        Sprite = GetNode<Sprite>(_spritePath);
    }
}
