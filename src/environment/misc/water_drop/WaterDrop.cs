using Godot;
using System;

public class WaterDrop : Area2D
{
    [Export(PropertyHint.Range, "1,10,0.5")] private float _fallSpeed = 1;
    [Export] private NodePath _animPlayerPath;

    private AnimationPlayer _animPlayer;
    private bool _freeze;

    private readonly float _timeFrozenUntilFreed = 3f;
    private float _timeFrozen;

    public override void _Ready()
    {
        _animPlayer = GetNode<AnimationPlayer>(_animPlayerPath);
        Connect("body_entered", this, nameof(OnBodyEntered));
    }

    // Called when the node enters the scene tree for the first time.
    public override void _PhysicsProcess(float delta)
    {
        if (!_freeze) Position = new Vector2(Position.x, Position.y + _fallSpeed);
        else
        {
            _timeFrozen += delta;
            if (_timeFrozen > _timeFrozenUntilFreed) QueueFree();
        }
    }

    private void OnBodyEntered(Node body)
    {
        if (_freeze) return;
        _animPlayer.Play("splash");
        _freeze = true;
    }
}
