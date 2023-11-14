using Godot;
using System;

public class BreakableItem : Node2D
{
    [Export] public float WeightRequiredToBreak { get; set; }
    [Export] private NodePath _spritePath;
    [Export] private NodePath _collisionShapePath;
    [Export] private NodePath _particlePath;

    private Sprite _sprite;
    private CollisionPolygon2D _collisionShape;
    private CPUParticles2D _particle;

    public override void _Ready()
    {
        _sprite = GetNode<Sprite>(_spritePath);
        _particle = GetNode<CPUParticles2D>(_particlePath);
        _collisionShape = GetNode<CollisionPolygon2D>(_collisionShapePath);
    }

    public void Break()
    {
        _sprite.Hide();
        _collisionShape.Disabled = true;
        _particle.Emitting = true;
        _particle.OneShot = true;
    }
}
