using Godot;
using System;
using System.Collections.Generic;

public class TrajectoryLine : Line2D
{
    [Export(PropertyHint.Range, "0,100,1")] private int _accuracy = 75;
    [Export] private NodePath _kinematicBodyPath;
    [Export] private NodePath _visibilityNotifierPath;
    [Export(PropertyHint.Layers2dPhysics)] private uint _emptyColLayer;
    [Export] public int MaxPointsPerLine { get; set; } = 750;

    public List<Vector2> CorePathPoints { get; private set; } = new List<Vector2>();
    
    private KinematicBody2D _kinematicBody;
    private uint _colLayer;
    private uint _colMask;
    private float _velocity;

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
        _velocity = (1 / (float)_accuracy) * 25000;
    }

    public KinematicCollision2D UpdateLine(Vector2 startPos, Vector2 direction, float delta)
    {
        ClearPoints();
        CorePathPoints.Clear();
        EnableCollisions();
        var pos = startPos;
        CollisionTestPosition = pos;
        var velocity = _velocity * direction * delta;
        CorePathPoints.Add(pos);
        for (var i = 0; i < MaxPointsPerLine; i++)
        {
            if (!IsOnScreen)
            {
                break;
            }
            
            AddPoint(pos);
            var collision = TestCollision(velocity);
            if (collision != null)
            {
                if (collision.Collider is BulletDeflector)
                {
                    velocity = velocity.Bounce(collision.Normal);
                    CorePathPoints.Add(pos + velocity);
                }
                else
                {
                    DisableCollisions();
                    CorePathPoints.Add(pos);
                    return collision;
                }
            }
            pos += velocity;
            CollisionTestPosition = pos;
        }
        if (CorePathPoints.Count < 2) CorePathPoints.Add(pos + (velocity * MaxPointsPerLine));
        return null;
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
