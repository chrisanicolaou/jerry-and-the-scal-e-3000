using Godot;
using System;

public class Bullet : KinematicBody2D
{
    [Export] public float Speed { get; set; }
    public Vector2 Direction { get; set;  }

    public override void _PhysicsProcess(float delta)
    {
        var velocity = Direction * Speed * delta;

        var collision = MoveAndCollide(velocity);
        // We've collided! DESTROY >:)
        if (collision?.Collider != null)
        {
            QueueFree();
        }
    }
}
