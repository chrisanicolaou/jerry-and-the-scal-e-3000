using Godot;
using System;
using GithubGameJam2023.player.player_gun;

public class Bullet : KinematicBody2D
{
    public float Speed { get; set; }
    public Vector2 Direction { get; set;  }
    public ScaleType Type { get; set; }

    public override void _PhysicsProcess(float delta)
    {
        var velocity = Direction * Speed * delta;

        var collision = MoveAndCollide(velocity, infiniteInertia: false);
        // We've collided! DESTROY >:)
        if (collision != null)
        {
            HandleCollision(collision);
        }
    }

    private void HandleCollision(KinematicCollision2D collision)
    {
        if (collision.Collider is BulletDeflector)
        {
            Direction = Direction.Bounce(collision.Normal);
            return;
        }
        if (collision.Collider is ScalableItem scalableItem)
        {
            scalableItem.OnBulletCollide(Type);
        }
        QueueFree();
    }
}
