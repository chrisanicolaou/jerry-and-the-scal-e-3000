using Godot;
using System;

public class PlayerGun : Node2D
{
    [Export] private NodePath _playerPath;
    [Export] private PackedScene _bulletScene;

    private Player _player;
    private float _radius;
    private Viewport _viewport;

    public override void _Ready()
    {
        _player = GetNode<Player>(_playerPath);
        _radius = Mathf.Abs(GlobalPosition.x - _player.GlobalPosition.x);
        _viewport = GetViewport();
    }

    public override void _Process(float delta)
    {
        var mousePos = _viewport.GetMousePosition();
        var direction = _player.GlobalPosition.DirectionTo(mousePos);
        GlobalPosition = _player.GlobalPosition + (_radius * direction);
        RotationDegrees = 90 * direction.y * (direction.x > 0 ? 1 : -1);
        var yScale = direction.x > 0 ? 1 : -1;
        if (Math.Abs(Scale.y - yScale) > 0.01)
        {
            Scale = new Vector2(yScale, yScale);
        }

        if (Input.IsActionPressed("shoot_big"))
        {
            var bullet = _bulletScene.Instance<Bullet>();
            bullet.GlobalPosition = GlobalPosition;
            bullet.Direction = direction;
            GetTree().Root.AddChild(bullet);
        }

        if (Input.IsActionPressed("shoot_small"))
        {
            var bullet = _bulletScene.Instance<Bullet>();
            bullet.GlobalPosition = GlobalPosition;
            bullet.Direction = new Vector2(direction);
            GetTree().Root.AddChild(bullet);
        }
    }
}
