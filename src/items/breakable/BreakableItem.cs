using Godot;
using System;

public class BreakableItem : Node2D
{
    [Export] public float WeightRequiredToBreak { get; set; }
    [Export] private NodePath _tileMapPath;
    [Export] private NodePath _particlePath;

    private TileMap _tileMap;
    private CPUParticles2D _particle;

    public override void _Ready()
    {
        _tileMap = GetNode<TileMap>(_tileMapPath);
        _particle = GetNode<CPUParticles2D>(_particlePath);
    }

    public void Break()
    {
        _tileMap.QueueFree();
        _particle.Emitting = true;
        _particle.OneShot = true;
    }
}
