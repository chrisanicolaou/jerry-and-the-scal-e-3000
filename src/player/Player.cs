using Godot;
using System;

public class Player : KinematicBody2D
{
    public bool Freeze { get; set; }
    [Export] private float _speed = 1;
    [Export] private float _jumpSpeed = 1;
    [Export] private float _jumpFloatyness = 0.1f;
    [Export] private float _gravityMultiplier = 1;
    [Export] private float _elongatedJumpMultiplier = 25;
    [Export] private float _snappyFallMultiplier = 25;
    private Vector2 _velocity;
    private float _gravity = Convert.ToInt32(ProjectSettings.GetSetting("physics/2d/default_gravity"));
    private float _timeInAir;
    public override void _PhysicsProcess(float delta)
    {
        if (Freeze) return;
        
        var direction = Input.GetAxis("left", "right");
        _velocity.x = Mathf.Abs(direction) > 0.001 ? direction * _speed : 0;

        MoveAndSlide(_velocity, Vector2.Up);

        if (!IsOnFloor())
        {
            _timeInAir += delta;
            _velocity.y += _gravity * _gravityMultiplier * delta;
            if (_velocity.y < 0)
            {
                if (Input.IsActionPressed("jump")) _velocity.y -= _elongatedJumpMultiplier * delta;
                _velocity.y += (1 / _jumpFloatyness) * _timeInAir;
            }
            else
            {
                _velocity.y += _snappyFallMultiplier * delta;
            }
        }
        else
        {
            _timeInAir = 0;
            _velocity.y = Input.IsActionPressed("jump") ? -_jumpSpeed : 0;
        }
    }
}
