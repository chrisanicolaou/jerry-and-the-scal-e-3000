using Godot;
using System;
using GithubGameJam2023.player.player_gun;

public class PlayerGun : Node2D
{
    [Signal] public delegate void ShotFired();
    [Export] private NodePath _playerPath;
    [Export] private PackedScene _bulletScene;
    [Export] public float BulletSpeed { get; set; }

    private Player _player;
    private float _radius;
    private Viewport _viewport;
    private Vector2 _aimDirection;
    private bool _disabled;
    private ScalableItemV2 _itemInScope;
    
    public TrajectoryLine TrajectoryLine { get; set; }

    public override void _Ready()
    {
        _player = GetNode<Player>(_playerPath);
        _radius = Mathf.Abs(GlobalPosition.x - _player.GlobalPosition.x);
        _viewport = GetViewport();
    }

    public override void _Process(float delta)
    {
        if (_player.Freeze || _disabled) return;
        
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
        var collision = TrajectoryLine.UpdateLine(GlobalPosition, direction, delta);
        if (collision?.Collider is ScalableItemV2 scalableItem && !scalableItem.IsMutated)
        {
            SetScopeOnItem(scalableItem);
        }
        else
        {
            _itemInScope?.RemoveOutline();
            _itemInScope = null;
        }
    }

    private void SetScopeOnItem(ScalableItemV2 scalableItem)
    {
        if (_itemInScope != scalableItem) _itemInScope?.RemoveOutline();
        _itemInScope = scalableItem;
        _itemInScope.AddOutline();
    }

    public void Disable()
    {
        _disabled = true;
        TrajectoryLine?.Hide();
        Hide();
    }

    public void Enable()
    {
        _disabled = false;
        TrajectoryLine?.Show();
        Show();
    }
}
