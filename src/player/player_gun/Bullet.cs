using Godot;
using System;
using GithubGameJam2023.player.player_gun;

public class Bullet : KinematicBody2D
{
    [Export] public float Speed { get; set; }
    [Export] public string ScaleGroupName { get; set; } = "scalable";
    public Vector2 Direction { get; set;  }
    public BulletType Type { get; set; }

    public override void _PhysicsProcess(float delta)
    {
        var velocity = Direction * Speed * delta;

        var collision = MoveAndCollide(velocity);
        // We've collided! DESTROY >:)
        if (collision != null)
        {
            var scalableItem = ((collision.Collider as Node)?.Owner as ScalableItem);
            scalableItem?.OnBulletCollide(Type);
            QueueFree();
        }
    }
}
