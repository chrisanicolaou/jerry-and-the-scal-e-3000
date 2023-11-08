using Godot;
using System;

public class TrajectoryLine : Line2D
{
    [Export] private NodePath _kinematicBodyPath;
    private KinematicBody2D _kinematicBody;

    public Vector2 CollisionTestPosition
    {
        get => _kinematicBody.Position;
        set => _kinematicBody.Position = value;
    }

    public override void _Ready()
    {
        _kinematicBody = GetNode<KinematicBody2D>(_kinematicBodyPath);
    }

    public KinematicCollision2D TestCollision(Vector2 vel)
    {
        return _kinematicBody.MoveAndCollide(vel, false, true, true);
    }
}
