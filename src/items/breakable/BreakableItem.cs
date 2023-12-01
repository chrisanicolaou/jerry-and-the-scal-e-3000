using Godot;
using System;
using ChiciStudios.GithubGameJam2023.Common.Audio;

public class BreakableItem : Node2D
{
    [Export] public float WeightRequiredToBreak { get; set; }
    [Export] private NodePath _tileMapPath;
    [Export] private NodePath _particlePath;
    [Export] private AudioStream _breakSfx;

    private AudioManager _audioManager;
    private TileMap _tileMap;
    private CPUParticles2D _particle;

    public override void _Ready()
    {
        _audioManager = GetNode<AudioManager>("/root/AudioManager");
        _tileMap = GetNode<TileMap>(_tileMapPath);
        _particle = GetNode<CPUParticles2D>(_particlePath);
    }

    public void Break()
    {
        _audioManager.PlaySfx(_breakSfx);
        _tileMap.QueueFree();
        _particle.Emitting = true;
        _particle.OneShot = true;
    }
}
