using Godot;
using System;
using System.Threading.Tasks;
using ChiciStudios.GithubGameJam2023.Common.Audio;
using GithubGameJam2023.player.player_gun;
using Godot.Collections;

public class Player : KinematicBody2D
{
    [Signal] public delegate void ShotsFired();
    [Signal] public delegate void ItemForcePutdown(ScalableItemV2 item);

    [Export] private Array<AudioStream> _jumpSfx;
    [Export] private bool _hasGun = true;
    [Export] private float _speed = 80;
    [Export] private float _jumpSpeed = 130;
    [Export] private float _jumpPlatformForgiveness = 0.2f;
    [Export] private float _jumpFloatyness = 0.02f;
    [Export] private float _gravityMultiplier = 1;
    [Export] private float _elongatedJumpMultiplier = 550;
    [Export] private float _maxFallSpeed = 250;
    [Export] private float _snappyFallMultiplier = 750;
    [Export] private Vector2 _itemThrowStrength;
    [Export] private Vector2 _carryOffset = Vector2.Zero;
    [Export] private NodePath _animPlayerPath;
    [Export] private NodePath _spriteNodePath;
    [Export] private NodePath _gunPath;
    [Export] private NodePath _trajectoryLinePath;
    [Export] private NodePath _laserPath;
    [Export] private AudioStream _dropItemSfx;
    [Export] private AudioStream _pickUpItemSfx;
    [Export] private AudioStream _laserNoEnergySfx;

    private AudioManager _audioManager;
    private Vector2 _velocity;
    private float _gravity = Convert.ToInt32(ProjectSettings.GetSetting("physics/2d/default_gravity"));
    private float _timeInAir;
    private bool _hasJustJumped;
    private bool _holdingJump;
    private AnimationPlayer _animPlayer;
    private Sprite _sprite;
    private PlayerGun _gun;
    
    public bool Freeze { get; set; }
    public ScalableItemV2 ItemCarry { get; set; }
    public int NumOfBullets { get; set; } = -2;
    public bool CanShoot => NumOfBullets > 0 || NumOfBullets < -1;

    public override void _Ready()
    {
        _audioManager = GetNode<AudioManager>("/root/AudioManager");
        _animPlayer = GetNode<AnimationPlayer>(_animPlayerPath);
        _sprite = GetNode<Sprite>(_spriteNodePath);
        _gun = GetNode<PlayerGun>(_gunPath);
        if (_trajectoryLinePath != null) _gun.TrajectoryLine = GetNode<TrajectoryLine>(_trajectoryLinePath);
        _gun.Laser = GetNode<Laser>(_laserPath);

        if (!_hasGun) DisableGun();
        _animPlayer.Play("idle");
    }

    private void DisableGun()
    {
        _hasGun = false;
        _gun.Disable();
    }

    private void EnableGun()
    {
        _hasGun = true;
        _gun.Enable();
    }

    public async Task EnterLevel(Door door)
    {
        var playerScale = 0.5f;
        var exitDoorDuration = 0.5f;
        
        GlobalPosition = door.GlobalPosition;
        var playerOriginalAlpha = Modulate.a;
        Modulate = new Color(Modulate.r, Modulate.g, Modulate.b, 0);
        var playerOriginalScale = Scale;
        Scale = new Vector2(playerScale, playerScale);
        
        PlayAnimationIfNotPlaying("enter_level");
        var tween = GetTree().CreateTween().SetParallel();
        tween.TweenProperty(this, "scale", playerOriginalScale, exitDoorDuration);
        tween.TweenProperty(this, "modulate:a", playerOriginalAlpha, exitDoorDuration);
        await ToSignal(tween, "finished");
        PlayAnimationIfNotPlaying("idle");
    }
    
    public async Task ExitLevel(Door door)
    {
        var playerScale = 0.5f;
        var exitDoorDuration = 0.5f;
        
        GlobalPosition = door.GlobalPosition;
        
        PlayAnimationIfNotPlaying("exit_level");
        var tween = GetTree().CreateTween().SetParallel();
        tween.TweenProperty(this, "scale", new Vector2(playerScale, playerScale), exitDoorDuration);
        tween.TweenProperty(this, "modulate:a", 0f, exitDoorDuration);
        await ToSignal(tween, "finished");
    }

    public void PickupItem(ScalableItemV2 item)
    {
        _audioManager.PlaySfx(_pickUpItemSfx);
        ItemCarry = item;
        // ItemCarry.Mode = RigidBody2D.ModeEnum.Kinematic;
        ItemCarry.SwitchToHeldCollisionLayer();
        // _sprite.AddChild(item);
        ItemCarry.GlobalPosition = new Vector2(_sprite.FlipH ? _sprite.GlobalPosition.x - ItemCarry.CarryOffset.x - _carryOffset.x : _sprite.GlobalPosition.x + ItemCarry.CarryOffset.x + _carryOffset.x,
            _sprite.GlobalPosition.y + ItemCarry.CarryOffset.y + _carryOffset.y);
    }

    public ScalableItemV2 PutdownItem()
    {
        _audioManager.PlaySfx(_dropItemSfx);
        var item = ItemCarry;
        // _sprite.RemoveChild(item);
        item.GlobalPosition = new Vector2(_sprite.FlipH ? _sprite.GlobalPosition.x - ItemCarry.CarryOffset.x - _carryOffset.x : _sprite.GlobalPosition.x + ItemCarry.CarryOffset.x + _carryOffset.x,
            _sprite.GlobalPosition.y + ItemCarry.CarryOffset.y + _carryOffset.y);
        item.Mode = RigidBody2D.ModeEnum.Rigid;
        item.LinearVelocity = Vector2.Zero;
        item.ApplyCentralImpulse(_sprite.FlipH ? -_itemThrowStrength : _itemThrowStrength);
        ItemCarry.SwitchToNormalCollisionLayer();
        ItemCarry = null;
        return item;
    }

    public void PickupGun()
    {
        EnableGun();
    }

    public void ToggleTrajectoryLine(bool value)
    {
        _gun.TrajectoryLine.Visible = value;
    }

    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (Freeze) return;
        
        if (inputEvent.IsActionPressed("shoot_big") && _hasGun)
        {
            if (!CanShoot)
            {
                _audioManager.PlaySfx(_laserNoEnergySfx);
                return;
            }
            _gun.Fire(ScaleType.Big);
            // GetTree().Root.AddChild(bullet);
            NumOfBullets--;
            EmitSignal(nameof(ShotsFired));
        }

        if (inputEvent.IsActionPressed("shoot_small") && _hasGun)
        {
            if (!CanShoot)
            {
                _audioManager.PlaySfx(_laserNoEnergySfx);
                return;
            }
            _gun.Fire(ScaleType.Small);
            // GetTree().Root.AddChild(bullet);
            NumOfBullets--;
            EmitSignal(nameof(ShotsFired));
        }

        if (inputEvent.IsActionPressed("jump"))
        {
            if (_timeInAir < _jumpPlatformForgiveness && !_hasJustJumped) Jump();
            _holdingJump = true;
        }

        if (inputEvent.IsActionReleased("jump")) _holdingJump = false;
    }

    public override void _PhysicsProcess(float delta)
    {
        if (Freeze) return;
        
        var direction = Input.GetAxis("left", "right");
        if (direction > 0.001)
        {
            _sprite.FlipH = false;
        }

        if (direction < -0.001)
        {
            _sprite.FlipH = true;
        }
        
        if (ItemCarry != null)
        {
            if (!ItemCarry.CanBeCarried)
            {
                var item = PutdownItem();
                EmitSignal(nameof(ItemForcePutdown), item);
            }
            else
            {
                ItemCarry.GlobalPosition = new Vector2(_sprite.FlipH ? _sprite.GlobalPosition.x - ItemCarry.CarryOffset.x - _carryOffset.x : _sprite.GlobalPosition.x + ItemCarry.CarryOffset.x + _carryOffset.x,
                    _sprite.GlobalPosition.y + ItemCarry.CarryOffset.y + _carryOffset.y);
            }
        }

        if (IsOnFloor())
        {
            PlayAnimationIfNotPlaying(Mathf.Abs(direction) > 0.001 ? "walk" : "idle");
        }
        
        _velocity.x = direction * _speed;

        // Correction to allow walking smoothly over slopes
        // var correction = Vector2.Zero;
        // if (IsOnFloor())
        // {
        //     correction = new Vector2(0, -1) - GetFloorNormal();
        //     correction *= -_gravity;
        // }
        
        MoveAndSlide(_velocity, Vector2.Up, true, infiniteInertia: false);

        if (!IsOnFloor())
        {
            _timeInAir += delta;
            if (IsOnCeiling()) _velocity.y = 0;
            _velocity.y += _gravity * _gravityMultiplier * delta;
            if (_velocity.y < 0)
            {
                if (_holdingJump)
                {
                    _velocity.y -= _elongatedJumpMultiplier * delta;
                }
                _velocity.y += (1 / _jumpFloatyness) * _timeInAir;
            }
            else
            {
                _velocity.y = Mathf.Min(_velocity.y + _snappyFallMultiplier * delta, _maxFallSpeed);
                // if (Input.IsActionPressed("jump") && _timeInAir < _jumpPlatformForgiveness && !_hasJustJumped)
                // {
                //     Jump();
                // }
            }
        }
        else
        {
            _timeInAir = 0;
            _hasJustJumped = false;
            // _velocity.y = 0;
            // if (Input.IsActionPressed("jump"))
            // {
            //     Jump();
            // }
            // else
            // {
            //     _velocity.y = 0;
            // }
        }
    }

    private void PlayAnimationIfNotPlaying(string animName)
    {
        if (_animPlayer.CurrentAnimation == animName) return;
        _animPlayer.Play(animName);
    }

    private void Jump()
    {
        _audioManager.PlaySfx(_jumpSfx[(int)(GD.Randi() % _jumpSfx.Count)]);
        _velocity.y = -_jumpSpeed;
        _timeInAir = 0;
        _hasJustJumped = true;
    }
}
