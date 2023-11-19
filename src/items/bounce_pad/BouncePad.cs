using Godot;
using System;

public class BouncePad : StaticBody2D
{
    [Export] private Vector2 _bounceForce;
    [Export] private NodePath _areaPath;

    public override void _Ready()
    {
        var area = GetNode<Area2D>(_areaPath);
        area.Connect("body_entered", this, nameof(OnBodyEntered));
    }

    public void OnBodyEntered(Node body)
    {
        if (body is ScalableItemV2 scalableItem)
        {
            scalableItem.ApplyCentralImpulse(_bounceForce);
        }
    }
}
