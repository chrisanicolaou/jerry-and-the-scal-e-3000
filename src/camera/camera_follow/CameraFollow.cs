using Godot;
using System;

public class CameraFollow : Camera2D
{
    [Export] private NodePath _followTargetPath;
    [Export] private Vector2 _offset;
    [Export] private float _lerpSpeed = 5;
    [Export] private Vector2 _horizontalLevelBounds;
    [Export] private Vector2 _verticalLevelBounds;
    
    private Node2D _followTarget;

    public override void _Ready()
    {
        _followTarget = GetNode<Node2D>(_followTargetPath);
        var targetPos = _followTarget.GlobalPosition + _offset;
        GlobalPosition = new Vector2(Mathf.Clamp(targetPos.x, _horizontalLevelBounds.x, _horizontalLevelBounds.y),
            Mathf.Clamp(targetPos.y, _verticalLevelBounds.x, _verticalLevelBounds.y));
    }

    public override void _Process(float delta)
    {
        if (_followTarget == null) return;
        var targetPos = _followTarget.GlobalPosition + _offset;
        var clampedTargetPos = new Vector2(Mathf.Clamp(targetPos.x, _horizontalLevelBounds.x, _horizontalLevelBounds.y),
            Mathf.Clamp(targetPos.y, _verticalLevelBounds.x, _verticalLevelBounds.y));

        GlobalPosition = _lerpSpeed > 0 ? GlobalPosition.LinearInterpolate(clampedTargetPos, delta * _lerpSpeed) : clampedTargetPos;
    }
}