using Godot;
using System;
using GithubGameJam2023.player.player_gun;

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

        if (Input.IsActionJustPressed("shoot_big"))
        {
            var bullet = _bulletScene.Instance<Bullet>();
            bullet.Modulate = Colors.Green;
            bullet.GlobalPosition = GlobalPosition;
            bullet.Direction = direction;
            bullet.Type = BulletType.ShootBig;
            GetTree().Root.AddChild(bullet);
        }

        if (Input.IsActionJustPressed("shoot_small"))
        {
            var bullet = _bulletScene.Instance<Bullet>();
            bullet.Modulate = Colors.Red;
            bullet.GlobalPosition = GlobalPosition;
            bullet.Direction = new Vector2(direction);
            bullet.Type = BulletType.ShootSmall;
            GetTree().Root.AddChild(bullet);
        }
    }
}
