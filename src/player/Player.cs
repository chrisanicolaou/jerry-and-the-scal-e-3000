using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export] private float _speed = 1;
    [Export] private float _jumpSpeed = 1;
    private Vector2 _velocity;
    private float _gravity = Convert.ToInt32(ProjectSettings.GetSetting("physics/2d/default_gravity"));
    public override void _PhysicsProcess(float delta)
    {
        var direction = Input.GetAxis("left", "right");
        _velocity.x = Mathf.Abs(direction) > 0.001 ? direction * _speed : 0;

        MoveAndSlide(_velocity, Vector2.Up);

        if (!IsOnFloor())
        {
            _velocity.y += _gravity * delta;
        }

        if (IsOnFloor())
        {
            _velocity.y = Input.IsActionJustPressed("jump") ? -_jumpSpeed : 0;
        }
    }
}
