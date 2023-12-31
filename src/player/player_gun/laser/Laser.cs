using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ChiciStudios.GithubGameJam2023.Common.Audio;
using GithubGameJam2023.player.player_gun;

public class Laser : Line2D
{
    [Export] private Color _scaleUpCol;
    [Export] private AudioStream _scaleUpSfx;
    [Export] private Color _scaleDownCol;
    [Export] private AudioStream _scaleDownSfx;
    [Export] private float _width = 10f;
    [Export] private float _holdDuration = 0.5f;
    [Export] private float _powerUpDuration = 0.3f;
    [Export] private Tween.TransitionType _transType = Tween.TransitionType.Cubic;
    [Export] private Tween.EaseType _ease = Tween.EaseType.InOut;
    [Export] private float _particleStartDelay = 0.1f;
    [Export] private float _particleImpactMultiplier = 1.5f;
    [Export] private PackedScene _laserContactParticlesScene;
    [Export] private PackedScene _laserBeamParticlesScene;
    [Export] private bool _enableBeamParticles;

    private AudioManager _audioManager;

    public override void _Ready()
    {
        _audioManager = GetNode<AudioManager>("/root/AudioManager");
        Hide();
    }

    public async Task ActivateLaser(List<(Vector2, Vector2)> corePathPoints, ScaleType type)
    {
        Show();
        _audioManager.PlaySfx(type == ScaleType.Big ? _scaleUpSfx : _scaleDownSfx);
        var color = type == ScaleType.Big ? _scaleUpCol : _scaleDownCol;
        DefaultColor = color;
        Points = corePathPoints.Select(cp => cp.Item1).ToArray();
        var tween = GetTree().CreateTween().SetTrans(_transType).SetEase(_ease);
        tween.TweenProperty(this, "width", _width, _powerUpDuration).From(0f);
        await ToSignal(GetTree().CreateTimer(_particleStartDelay, false), "timeout");
        var particles = new List<CPUParticles2D>();
        (Vector2, Vector2)? previousPoint = null;
        foreach (var point in corePathPoints)
        {
            var particle = _laserContactParticlesScene.Instance<CPUParticles2D>();
            particle.Position = point.Item1;
            particle.InitialVelocity = previousPoint == null ? 100 : 200;
            // particle.ProcessMaterial.Set("color", color);
            particle.Color = color;
            AddChild(particle);
            particle.OneShot = true;
            particle.Emitting = true;
            particle.GlobalRotation = point.Item2.Angle();
            particles.Add(particle);
            if (previousPoint != null && _enableBeamParticles)
            {
                var prevPoint = previousPoint.Value;
                var beamParticle = _laserBeamParticlesScene.Instance<CPUParticles2D>();
                beamParticle.Position = (prevPoint.Item1 + point.Item1) / 2;
                beamParticle.GlobalRotation = prevPoint.Item2.Angle();
                var distFactor = point.Item1.DistanceTo(prevPoint.Item1) / 2;
                beamParticle.EmissionRectExtents = new Vector2(distFactor, Width);
                beamParticle.Color = color;
                AddChild(beamParticle);
                beamParticle.OneShot = true;
                beamParticle.Emitting = true;
                particles.Add(beamParticle);
            }
            previousPoint = point;
        }
        await ToSignal(GetTree().CreateTimer(_holdDuration, false), "timeout");
        tween = GetTree().CreateTween().SetTrans(_transType).SetEase(_ease);
        tween.TweenProperty(this, "width", 0f, _powerUpDuration).From(_width);
        await ToSignal(tween, "finished");
        particles.ForEach(p => p.QueueFree());
        Hide();
    }
}
