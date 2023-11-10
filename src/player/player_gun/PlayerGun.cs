using Godot;
using System;
using GithubGameJam2023.player.player_gun;

public class PlayerGun : Node2D
{
    [Signal] public delegate void ShotFired();
    [Export] private NodePath _playerPath;
    [Export] private PackedScene _bulletScene;
    [Export] public float BulletSpeed { get; set; }
    
    private const float MaxPointsPerLine = 750;

    private Player _player;
    private float _radius;
    private Viewport _viewport;
    private Vector2 _aimDirection;
    
    public TrajectoryLine TrajectoryLine { get; set; }

    public override void _Ready()
    {
        _player = GetNode<Player>(_playerPath);
        _radius = Mathf.Abs(GlobalPosition.x - _player.GlobalPosition.x);
        _viewport = GetViewport();
    }

    public override void _Process(float delta)
    {
        if (_player.Freeze) return;
        
        var mousePos = GetGlobalMousePosition();
        _aimDirection = _player.GlobalPosition.DirectionTo(mousePos);
        GlobalPosition = _player.GlobalPosition + (_radius * _aimDirection);
        RotationDegrees = 90 * _aimDirection.y * (_aimDirection.x > 0 ? 1 : -1);
        if (TrajectoryLine != null) UpdateBulletTrajectory(_aimDirection, delta);
        var yScale = _aimDirection.x > 0 ? 1 : -1;
        if (Math.Abs(Scale.y - yScale) > 0.01)
        {
            Scale = new Vector2(yScale, yScale);
        }
    }

    public Bullet CreateBullet(ScaleType type)
    {
        var bullet = _bulletScene.Instance<Bullet>();
        bullet.Modulate = type == ScaleType.Big ? Colors.Aqua : Colors.Fuchsia;
        bullet.GlobalPosition = GlobalPosition;
        bullet.Direction = _aimDirection;
        bullet.Speed = BulletSpeed;
        bullet.Type = type;
        return bullet;
    }

    public void UpdateBulletTrajectory(Vector2 direction, float delta)
    {
        TrajectoryLine.ClearPoints();
        TrajectoryLine.EnableCollisions();
        var pos = GlobalPosition;
        TrajectoryLine.CollisionTestPosition = pos;
        var velocity = BulletSpeed * direction * delta;
        for (var i = 0; i < MaxPointsPerLine; i++)
        {
            if (!TrajectoryLine.IsOnScreen) break;
            
            TrajectoryLine.AddPoint(pos);
            var collision = TrajectoryLine.TestCollision(velocity);
            if (collision != null)
            {
                if (collision.Collider is BulletDeflector)
                {
                    velocity = velocity.Bounce(collision.Normal);
                }
                else
                {
                    TrajectoryLine.DisableCollisions();
                    break;
                }
            }
            pos += velocity;
            TrajectoryLine.CollisionTestPosition = pos;
        }
    }
}
