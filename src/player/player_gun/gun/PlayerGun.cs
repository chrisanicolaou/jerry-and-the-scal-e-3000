using Godot;
using System;
using System.Threading.Tasks;
using GithubGameJam2023.player.player_gun;

public class PlayerGun : Node2D
{
    [Signal] public delegate void ShotFired();
    [Export] private NodePath _playerPath;
    [Export] private Vector2 _playerOffset;
    [Export] private NodePath _trajectoryLinePointPath;

    private Player _player;
    private Viewport _viewport;
    private Vector2 _aimDirection;
    private bool _disabled;
    private ScalableItemV2 _itemInScope;
    private Node2D _trajectoryLinePoint;
    
    public TrajectoryLine TrajectoryLine { get; set; }
    public Laser Laser { get; set; }

    public override void _Ready()
    {
        _player = GetNode<Player>(_playerPath);
        _trajectoryLinePoint = GetNode<Node2D>(_trajectoryLinePointPath);
        _viewport = GetViewport();
    }

    public override void _Process(float delta)
    {
        if (_player.Freeze || _disabled) return;
        
        var mousePos = GetGlobalMousePosition();
        _aimDirection = _player.GlobalPosition.DirectionTo(mousePos);
        GlobalPosition = (_player.GlobalPosition + (_playerOffset.x * _aimDirection));
        GlobalPosition = new Vector2(GlobalPosition.x, GlobalPosition.y + _playerOffset.y);
        RotationDegrees = 90 * _aimDirection.y * (_aimDirection.x > 0 ? 1 : -1);
        if (TrajectoryLine != null) UpdateBulletTrajectory(_aimDirection, delta);
        var yScale = _aimDirection.x > 0 ? 1 : -1;
        if (Math.Abs(Scale.y - yScale) > 0.01)
        {
            Scale = new Vector2(yScale, yScale);
        }
    }

    public async Task Fire(ScaleType type)
    {
        // UpdateBulletTrajectory(_aimDirection, 0.001f, true);
        Laser.ActivateLaser(TrajectoryLine.CorePathPoints, type);
        HandleCollision(type);
        // var bullet = _bulletScene.Instance<Bullet>();
        // bullet.Modulate = type == ScaleType.Big ? Colors.Aqua : Colors.Fuchsia;
        // bullet.GlobalPosition = GlobalPosition;
        // bullet.Direction = _aimDirection;
        // bullet.Speed = BulletSpeed;
        // bullet.Type = type;
        // return bullet;
    }

    private void HandleCollision(ScaleType type)
    {
        if (_itemInScope != null) _itemInScope.OnBulletCollide(type);
    }

    public void UpdateBulletTrajectory(Vector2 direction, float delta, bool continueOffScreen = false)
    {
        var collision = TrajectoryLine.UpdateLine(_trajectoryLinePoint.GlobalPosition, direction, delta, continueOffScreen);
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
