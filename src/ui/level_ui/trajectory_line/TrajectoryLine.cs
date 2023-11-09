using Godot;
using System;

public class TrajectoryLine : Line2D
{
    [Export] private NodePath _kinematicBodyPath;
    private KinematicBody2D _kinematicBody;
    [Export(PropertyHint.Layers2dPhysics)] private uint _emptyColLayer;

    private uint _colLayer;
    private uint _colMask;

    public Vector2 CollisionTestPosition
    {
        get => _kinematicBody.Position;
        set => _kinematicBody.Position = value;
    }

    public override void _Ready()
    {
        _kinematicBody = GetNode<KinematicBody2D>(_kinematicBodyPath);
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
}
