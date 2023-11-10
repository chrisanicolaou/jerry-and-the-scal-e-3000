using Godot;
using System;

public class TrajectoryLine : Line2D
{
    [Export] private NodePath _kinematicBodyPath;
    [Export] private NodePath _visibilityNotifierPath;
    private KinematicBody2D _kinematicBody;
    [Export(PropertyHint.Layers2dPhysics)] private uint _emptyColLayer;

    private uint _colLayer;
    private uint _colMask;

    public Vector2 CollisionTestPosition
    {
        get => _kinematicBody.Position;
        set => _kinematicBody.Position = value;
    }
    
    public bool IsOnScreen { get; private set; }

    public override void _Ready()
    {
        _kinematicBody = GetNode<KinematicBody2D>(_kinematicBodyPath);
        var visibilityNotifier = GetNode<VisibilityNotifier2D>(_visibilityNotifierPath);
        visibilityNotifier.Connect("screen_exited", this, nameof(OnScreenExit));
        visibilityNotifier.Connect("screen_entered", this, nameof(OnScreenEnter));
        _colLayer = _kinematicBody.CollisionLayer;
        _colMask = _kinematicBody.CollisionMask;
    }

    public void DisableCollisions()
    {
        _kinematicBody.CollisionLayer = _emptyColLayer;
        _kinematicBody.CollisionMask = _emptyColLayer;
    }

    public void EnableCollisions()
    {
        _kinematicBody.CollisionLayer = _colLayer;
        _kinematicBody.CollisionMask = _colMask;
    }

    public KinematicCollision2D TestCollision(Vector2 vel)
    {
        return _kinematicBody.MoveAndCollide(vel, false, testOnly: true);
    }

    private void OnScreenEnter() => IsOnScreen = true;
    private void OnScreenExit() => IsOnScreen = false;
}
